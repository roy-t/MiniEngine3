using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Content.Shaders.Generated;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.ECS.Systems;

namespace Mini.Engine.Graphics.Lighting.ImageBasedLights;

[Service]
public sealed partial class ImageBasedLightSystem : ISystem, IDisposable
{
    private readonly Device Device;
    private readonly DeferredDeviceContext Context;
    private readonly FrameService FrameService;
    private readonly FullScreenTriangle FullScreenTriangleShader;
    private readonly ImageBasedLight Shader;
    private readonly ImageBasedLight.User User;

    private readonly ITexture2D BrdfLut;

    public ImageBasedLightSystem(Device device, FrameService frameService, BrdfLutGenerator generator, FullScreenTriangle fullScreenTriangleShader, ImageBasedLight shader)
    {
        this.Device = device;
        this.Context = device.CreateDeferredContextFor<ImageBasedLightSystem>();
        this.FrameService = frameService;
        this.FullScreenTriangleShader = fullScreenTriangleShader;
        this.Shader = shader;
        this.User = shader.CreateUserFor<ImageBasedLightSystem>();

        this.BrdfLut = generator.Generate(nameof(ImageBasedLightSystem));
    }

    public void OnSet()
    {
        var blendState = this.Device.BlendStates.Additive;
        var depthStencilState = this.Device.DepthStencilStates.None;
        this.Context.SetupFullScreenTriangle(this.FullScreenTriangleShader.TextureVs, this.Shader.Ps, blendState, depthStencilState);

        this.Context.OM.SetRenderTarget(this.FrameService.LBuffer.Light);

        this.Context.PS.SetSampler(0, this.Device.SamplerStates.LinearClamp);
        this.Context.PS.SetShaderResourceN(ImageBasedLight.Albedo, this.FrameService.GBuffer.Albedo);
        this.Context.PS.SetShaderResourceN(ImageBasedLight.Normal, this.FrameService.GBuffer.Normal);
        this.Context.PS.SetShaderResourceN(ImageBasedLight.Depth, this.FrameService.GBuffer.DepthStencilBuffer);
        this.Context.PS.SetShaderResourceN(ImageBasedLight.Material, this.FrameService.GBuffer.Material);
        this.Context.PS.SetShaderResource(ImageBasedLight.BrdfLut, this.BrdfLut);

        var camera = this.FrameService.GetPrimaryCamera().Camera;
        var cameraTransform = this.FrameService.GetPrimaryCameraTransform().Transform;

        var viewProjection = camera.GetViewProjection(cameraTransform);
        Matrix4x4.Invert(viewProjection, out var inverseViewProjection);
        var cameraPosition = cameraTransform.GetPosition();
        this.User.MapConstants(this.Context, inverseViewProjection, cameraPosition);

        this.Context.PS.SetConstantBuffer(ImageBasedLight.ConstantsSlot, this.User.ConstantsBuffer);
        this.Context.PS.SetConstantBuffer(ImageBasedLight.PerLightConstantsSlot, this.User.PerLightConstantsBuffer);
    }

    [Process(Query = ProcessQuery.All)]
    public void Render(ref SkyboxComponent skybox)
    {
        this.User.MapPerLightConstants(this.Context, skybox.EnvironmentLevels, skybox.Strength);

        this.Context.PS.SetShaderResource(ImageBasedLight.Irradiance, skybox.Irradiance);
        this.Context.PS.SetShaderResource(ImageBasedLight.Environment, skybox.Environment);

        this.Context.Draw(3);
    }

    public void OnUnSet()
    {
        using var commandList = this.Context.FinishCommandList();
        this.Device.ImmediateContext.ExecuteCommandList(commandList);
    }

    public void Dispose()
    {
        this.BrdfLut.Dispose();
        this.User.Dispose();
        this.Context.Dispose();
    }
}
