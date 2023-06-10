using System.Drawing;
using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.Content.Shaders.Generated;
using Mini.Engine.Content.Textures;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Contexts.States;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.ECS.Components;

namespace Mini.Engine.Graphics.Lighting.ImageBasedLights;

[Service]
public sealed class ImageBasedLightSystem : IDisposable
{
    private readonly BlendState BlendState;
    private readonly DepthStencilState DepthStencilState;
    private readonly SamplerState SamplerState;

    private readonly DeferredDeviceContext Context;
    private readonly FrameService FrameService;
    private readonly FullScreenTriangle FullScreenTriangleShader;
    private readonly ImageBasedLight Shader;
    private readonly ImageBasedLight.User User;

    private readonly IComponentContainer<SkyboxComponent> SkyboxContainer;

    private readonly ILifetime<ITexture> BrdfLut;

    public ImageBasedLightSystem(Device device, FrameService frameService, FullScreenTriangle fullScreenTriangleShader, ImageBasedLight shader, ContentManager contentManager, BrdfLutProcessor generator, IComponentContainer<SkyboxComponent> componentContainer)
    {
        this.BlendState = device.BlendStates.Additive;
        this.DepthStencilState = device.DepthStencilStates.None;
        this.SamplerState = device.SamplerStates.LinearClamp;

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

            foreach (ref var component in this.SkyboxContainer.IterateAll())
            {
                this.Render(in component.Value);
            }

            return this.Context.FinishCommandList();
        });
    }

    private void Setup(in Rectangle viewport, in Rectangle scissor)
    {        
        this.Context.SetupFullScreenTriangle(this.FullScreenTriangleShader.TextureVs, in viewport, in scissor, this.Shader.Ps, this.BlendState, this.DepthStencilState);

        this.Context.OM.SetRenderTarget(this.FrameService.LBuffer.Light);

        this.Context.PS.SetSampler(0, this.SamplerState);
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

    private void Render(in SkyboxComponent skybox)
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
