using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Content.Shaders.Generated;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.ECS.Systems;
using Mini.Engine.Graphics.Cameras;
using Mini.Engine.Graphics.Lighting.ImageBasedLights;

namespace Mini.Engine.Graphics;

[Service]
public sealed partial class SkyboxSystem : ISystem, IDisposable
{
    private readonly Device Device;
    private readonly DeferredDeviceContext Context;
    private readonly Skybox Shader;
    private readonly Skybox.User User;
    private readonly FrameService FrameService;

    public SkyboxSystem(Device device, FrameService frameService, Skybox shader)
    {
        this.Device = device;
        this.Context = device.CreateDeferredContextFor<SkyboxSystem>();
        this.Shader = shader;
        this.User = shader.CreateUserFor<SkyboxSystem>();
        this.FrameService = frameService;
    }

    public void OnSet()
    {
        var blend = this.Device.BlendStates.Opaque;
        var depth = this.Device.DepthStencilStates.ReverseZReadOnly;
        this.Context.SetupFullScreenTriangle(this.Shader.Vs, this.Shader.Ps, blend, depth);

        this.Context.PS.SetSampler(Skybox.TextureSampler, this.Device.SamplerStates.LinearClamp);

        this.Context.OM.SetRenderTargets(this.FrameService.GBuffer.DepthStencilBuffer, this.FrameService.LBuffer.Light);
    }

    [Process(Query = ProcessQuery.All)]
    public void DrawSkybox(ref SkyboxComponent skybox)
    {        
        var camera = this.FrameService.GetPrimaryCamera().Camera;
        var cameraTransform = this.FrameService.GetPrimaryCameraTransform().Current;
       
        var view = Matrix4x4.CreateLookAt(Vector3.Zero, cameraTransform.GetForward(), cameraTransform.GetUp());        
        var projection = ProjectionMatrix.InfiniteReversedZ(0.1f, MathF.PI / 2.0f, camera.AspectRatio);
        var worldViewProjection = view * projection;
        Matrix4x4.Invert(worldViewProjection, out var inverse);
        this.User.MapConstants(this.Context, inverse);
        this.Context.VS.SetConstantBuffer(Skybox.ConstantsSlot, this.User.ConstantsBuffer);

        this.Context.PS.SetShaderResource(Skybox.CubeMap, skybox.Albedo);
        this.Context.Draw(3);
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
