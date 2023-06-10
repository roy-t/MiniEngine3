using System.Drawing;
using System.Numerics;
using LibGame.Graphics;
using Mini.Engine.Configuration;
using Mini.Engine.Content.Shaders.Generated;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.ECS.Components;
using Mini.Engine.Graphics.Lighting.ImageBasedLights;

namespace Mini.Engine.Graphics;

[Service]
public sealed class SkyboxSystem : IDisposable
{
    private readonly Device Device;
    private readonly DeferredDeviceContext Context;
    private readonly Skybox Shader;
    private readonly Skybox.User User;
    private readonly FrameService FrameService;

    private readonly IComponentContainer<SkyboxComponent> SkyboxContainer;

    public SkyboxSystem(Device device, FrameService frameService, Skybox shader, IComponentContainer<SkyboxComponent> componentContainer)
    {
        this.Device = device;
        this.Context = device.CreateDeferredContextFor<SkyboxSystem>();
        this.Shader = shader;
        this.User = shader.CreateUserFor<SkyboxSystem>();
        this.FrameService = frameService;
        this.SkyboxContainer = componentContainer;
    }

    public Task<CommandList> Render(Rectangle viewport, Rectangle scissor)
    {
        return Task.Run(() =>
        {
            this.Setup(viewport, scissor);

            foreach (ref var component in this.SkyboxContainer.IterateAll())
            {
                this.DrawSkybox(in component.Value);
            }

            return this.Context.FinishCommandList();
        });
    }

    private void Setup(in Rectangle viewport, in Rectangle scissor)
    {
        var blend = this.Device.BlendStates.Opaque;
        var depth = this.Device.DepthStencilStates.ReverseZReadOnly;
        this.Context.SetupFullScreenTriangle(this.Shader.Vs, in viewport, in scissor, this.Shader.Ps, blend, depth);

        this.Context.PS.SetSampler(Skybox.TextureSampler, this.Device.SamplerStates.LinearClamp);

        this.Context.OM.SetRenderTargets(this.FrameService.GBuffer.DepthStencilBuffer, this.FrameService.LBuffer.Light);
    }

    private void DrawSkybox(in SkyboxComponent skybox)
    {
        ref var camera = ref this.FrameService.GetPrimaryCamera().Camera;
        ref var cameraTransform = ref this.FrameService.GetPrimaryCameraTransform().Current;

        var view = Matrix4x4.CreateLookAt(Vector3.Zero, cameraTransform.GetForward(), cameraTransform.GetUp());
        var projection = ProjectionMatrix.InfiniteReversedZ(0.1f, MathF.PI / 2.0f, camera.AspectRatio);
        var worldViewProjection = view * projection;
        Matrix4x4.Invert(worldViewProjection, out var inverse);
        this.User.MapConstants(this.Context, inverse);
        this.Context.VS.SetConstantBuffer(Skybox.ConstantsSlot, this.User.ConstantsBuffer);

        this.Context.PS.SetShaderResource(Skybox.CubeMap, skybox.Albedo);
        this.Context.Draw(3);
    }

    public void Dispose()
    {
        this.User.Dispose();
        this.Context.Dispose();
    }
}
