using System;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX;
using Mini.Engine.ECS.Systems;
using Mini.Engine.Content.Shaders;
using Mini.Engine.Content.Shaders.SunLight;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.Graphics.Lighting.PointLights;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.Graphics.Transforms;
using System.Numerics;

namespace Mini.Engine.Graphics.Lighting.ShadowingLights;

[Service]
public sealed partial class SunLightSystem : ISystem, IDisposable
{
    private readonly Device Device;
    private readonly DeferredDeviceContext Context;
    private readonly FrameService FrameService;

    private readonly FullScreenTriangleTextureVs VertexShader;
    private readonly SunLightPs PixelShader;

    private readonly ConstantBuffer<Constants> ConstantBuffer;

    public SunLightSystem(Device device, FrameService frameService, FullScreenTriangleTextureVs vertexShader, SunLightPs pixelShader)
    {
        this.Device = device;
        this.Context = device.CreateDeferredContextFor<SunLightSystem>();
        this.FrameService = frameService;
        this.VertexShader = vertexShader;
        this.PixelShader = pixelShader;

        this.ConstantBuffer = new ConstantBuffer<Constants>(device, $"{nameof(SunLightSystem)}_CB");
    }

    public void OnSet()
    {
        this.Context.SetupFullScreenTriangle(this.VertexShader, this.PixelShader, this.Device.BlendStates.Additive, this.Device.DepthStencilStates.None);
        this.Context.OM.SetRenderTarget(this.FrameService.LBuffer.Light);

        this.Context.PS.SetSampler(SunLight.TextureSampler, this.Device.SamplerStates.LinearClamp);
        this.Context.PS.SetShaderResource(SunLight.Albedo, this.FrameService.GBuffer.Albedo);
        this.Context.PS.SetShaderResource(SunLight.Normal, this.FrameService.GBuffer.Normal);
        this.Context.PS.SetShaderResource(SunLight.Depth, this.FrameService.GBuffer.DepthStencilBuffer);
        this.Context.PS.SetShaderResource(SunLight.Material, this.FrameService.GBuffer.Material);

        this.Context.PS.SetSampler(SunLight.ShadowSampler, this.Device.SamplerStates.CompareLessEqualClamp);
        
        this.Context.PS.SetConstantBuffer(Constants.Slot, this.ConstantBuffer);
    }

    [Process(Query = ProcessQuery.All)]
    public void DrawSunLight(SunLightComponent sunlight, CascadedShadowMapComponent shadowMap, TransformComponent viewPoint)
    {
        var camera = this.FrameService.Camera;
        Matrix4x4.Invert(camera.ViewProjection, out var inverse);

        var shadow = new ShadowProperties()
        {
            Offsets = Pack(shadowMap.Offsets),
            Scales = Pack(shadowMap.Scales),
            Splits = Pack(shadowMap.Splits),
            ShadowMatrix = shadowMap.GlobalShadowMatrix
        };

        var constants = new Constants()
        {
            CameraPosition = camera.Transform.Position,
            Color = sunlight.Color,
            Strength = sunlight.Strength,
            SurfaceToLight = -viewPoint.Transform.Forward,
            InverseViewProjection = inverse,
            Shadow = shadow
        };

        this.ConstantBuffer.MapData(this.Context, constants);

        this.Context.PS.SetShaderResource(SunLight.ShadowMap, shadowMap.RenderTargetArray);

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
        this.ConstantBuffer.Dispose();
        this.Context.Dispose();
    }
}
