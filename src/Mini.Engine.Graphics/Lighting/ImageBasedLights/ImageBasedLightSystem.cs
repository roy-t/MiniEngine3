using System.Drawing;
using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.Content.Shaders.Generated;
using Mini.Engine.Content.Textures;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.ECS.Components;

namespace Mini.Engine.Graphics.Lighting.ImageBasedLights;

[Service]
public sealed class ImageBasedLightSystem : IDisposable
{
    private readonly Device Device;
    private readonly DeferredDeviceContext Context;
    private readonly FrameService FrameService;
    private readonly FullScreenTriangle FullScreenTriangleShader;
    private readonly ImageBasedLight Shader;
    private readonly ImageBasedLight.User User;

    private readonly IComponentContainer<SkyboxComponent> SkyboxContainer;

    private readonly ILifetime<ITexture> BrdfLut;

    public ImageBasedLightSystem(Device device, FrameService frameService, FullScreenTriangle fullScreenTriangleShader, ImageBasedLight shader, ContentManager contentManager, BrdfLutProcessor generator, IComponentContainer<SkyboxComponent> componentContainer)
    {
        this.Device = device;
        this.Context = device.CreateDeferredContextFor<ImageBasedLightSystem>();
        this.FrameService = frameService;
        this.FullScreenTriangleShader = fullScreenTriangleShader;
        this.Shader = shader;
        this.User = shader.CreateUserFor<ImageBasedLightSystem>();

        this.BrdfLut = contentManager.Load(generator, new ContentId("brdflut.hdr"), TextureSettings.RenderData);
        this.SkyboxContainer = componentContainer;
    }

    public Task<CommandList> Render(Rectangle viewport, Rectangle scissor)
    {
        return Task.Run(() =>
        {
            this.Setup(viewport, scissor);

            foreach (ref var skybox in this.SkyboxContainer.IterateAll())
            {
                Render(ref skybox.Value);
            }

            return this.Context.FinishCommandList();
        });
    }

    private void Setup(in Rectangle viewport, in Rectangle scissor)
    {
        var blendState = this.Device.BlendStates.Additive;
        var depthStencilState = this.Device.DepthStencilStates.None;
        this.Context.SetupFullScreenTriangle(this.FullScreenTriangleShader.TextureVs, in viewport, in scissor, this.Shader.Ps, blendState, depthStencilState);

        this.Context.OM.SetRenderTarget(this.FrameService.LBuffer.Light);

        this.Context.PS.SetSampler(0, this.Device.SamplerStates.LinearClamp);
        this.Context.PS.SetShaderResource(ImageBasedLight.Albedo, this.FrameService.GBuffer.Albedo);
        this.Context.PS.SetShaderResource(ImageBasedLight.Normal, this.FrameService.GBuffer.Normal);
        this.Context.PS.SetShaderResource(ImageBasedLight.Depth, this.FrameService.GBuffer.DepthStencilBuffer);
        this.Context.PS.SetShaderResource(ImageBasedLight.Material, this.FrameService.GBuffer.Material);
        this.Context.PS.SetShaderResource(ImageBasedLight.BrdfLut, this.BrdfLut);

        ref var camera = ref this.FrameService.GetPrimaryCamera();
        ref var cameraTransform = ref this.FrameService.GetPrimaryCameraTransform().Current;

        var viewProjection = camera.Camera.GetInfiniteReversedZViewProjection(in cameraTransform, camera.Jitter);
        Matrix4x4.Invert(viewProjection, out var inverseViewProjection);
        var cameraPosition = cameraTransform.GetPosition();
        this.User.MapConstants(this.Context, inverseViewProjection, cameraPosition);

        this.Context.PS.SetConstantBuffer(ImageBasedLight.ConstantsSlot, this.User.ConstantsBuffer);
        this.Context.PS.SetConstantBuffer(ImageBasedLight.PerLightConstantsSlot, this.User.PerLightConstantsBuffer);
    }


    private void Render(ref SkyboxComponent skybox)
    {
        this.User.MapPerLightConstants(this.Context, skybox.EnvironmentLevels, skybox.Strength);

        this.Context.PS.SetShaderResource(ImageBasedLight.Irradiance, skybox.Irradiance);
        this.Context.PS.SetShaderResource(ImageBasedLight.Environment, skybox.Environment);

        this.Context.Draw(3);
    }

    public void Dispose()
    {
        this.User.Dispose();
        this.Context.Dispose();
    }
}
