using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Content.Shaders.Generated;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.ECS.Systems;
using Mini.Engine.Graphics.Transforms;

namespace Mini.Engine.Graphics.Lighting.ShadowingLights;

[Service]
public sealed partial class SunLightSystem : ISystem, IDisposable
{
    private readonly Device Device;
    private readonly DeferredDeviceContext Context;
    private readonly FrameService FrameService;

    private readonly FullScreenTriangle FullScreenTriangle;

    private readonly SunLight Shader;
    private readonly SunLight.User User;

    public SunLightSystem(Device device, FrameService frameService, FullScreenTriangle fullScreenTriangle, SunLight shader)
    {
        this.Device = device;
        this.Context = device.CreateDeferredContextFor<SunLightSystem>();
        this.FrameService = frameService;
        this.FullScreenTriangle = fullScreenTriangle;

        this.Shader = shader;
        this.User = shader.CreateUserFor<SunLightSystem>();
    }

    public void OnSet()
    {
        this.Context.SetupFullScreenTriangle(this.FullScreenTriangle.TextureVs, this.Shader.Ps, this.Device.BlendStates.Additive, this.Device.DepthStencilStates.None);
        this.Context.OM.SetRenderTarget(this.FrameService.LBuffer.Light);

        this.Context.PS.SetSampler(SunLight.TextureSampler, this.Device.SamplerStates.LinearClamp);
        this.Context.PS.SetShaderResource(SunLight.Albedo, this.FrameService.GBuffer.Albedo);
        this.Context.PS.SetShaderResource(SunLight.Normal, this.FrameService.GBuffer.Normal);
        this.Context.PS.SetShaderResource(SunLight.Depth, this.FrameService.GBuffer.DepthStencilBuffer);
        this.Context.PS.SetShaderResource(SunLight.Material, this.FrameService.GBuffer.Material);

        this.Context.PS.SetSampler(SunLight.ShadowSampler, this.Device.SamplerStates.CompareLessEqualClamp);

        this.Context.PS.SetConstantBuffer(SunLight.ConstantsSlot, this.User.ConstantsBuffer);
    }

    [Process(Query = ProcessQuery.All)]
    public void DrawSunLight(ref SunLightComponent sunlight, ref CascadedShadowMapComponent shadowMap, ref TransformComponent viewPoint)
    {
        var camera = this.FrameService.Camera;
        Matrix4x4.Invert(camera.ViewProjection, out var inverse);

        var shadow = new SunLight.ShadowProperties()
        {
            Offsets = Pack(shadowMap.Offsets),
            Scales = Pack(shadowMap.Scales),
            Splits = Pack(shadowMap.Splits),
            ShadowMatrix = shadowMap.GlobalShadowMatrix
        };

        this.User.MapConstants(this.Context, sunlight.Color, -viewPoint.Transform.Forward, sunlight.Strength, inverse, camera.Transform.Position, shadow);

        this.Context.PS.SetShaderResource(SunLight.ShadowMap, shadowMap.DepthBuffers);

        this.Context.Draw(3);
    }

    private static Matrix4x4 Pack(Vector4[] vectors)
    {
        return new Matrix4x4
        (
            vectors[0].X, vectors[1].X, vectors[2].X, vectors[3].X,
            vectors[0].Y, vectors[1].Y, vectors[2].Y, vectors[3].Y,
            vectors[0].Z, vectors[1].Z, vectors[2].Z, vectors[3].Z,
            vectors[0].W, vectors[1].W, vectors[2].W, vectors[3].W
        );
    }

    private static Vector4 Pack(float[] vectors)
    {
        return new Vector4(vectors[0], vectors[1], vectors[2], vectors[3]);
    }

    public void OnUnSet()
    {
        using var commandList = this.Context.FinishCommandList();
        this.Device.ImmediateContext.ExecuteCommandList(commandList);
    }

    public void Dispose()
    {
        this.User.Dispose();
        this.Context.Dispose();
    }
}
