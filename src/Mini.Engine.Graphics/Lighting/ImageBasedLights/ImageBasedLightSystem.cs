using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.Content.Shaders;
using Mini.Engine.Content.Shaders.ImageBasedLight;
using Mini.Engine.ECS.Systems;
using System;

namespace Mini.Engine.Graphics.Lighting.ImageBasedLights;

[Service]
public sealed partial class ImageBasedLightSystem : ISystem, IDisposable
{
    private readonly Device Device;
    private readonly DeferredDeviceContext Context;
    private readonly FullScreenTriangle FullScreenTriangle;
    private readonly FrameService FrameService;
    private readonly ImageBasedLightVs VertexShader;
    private readonly ImageBasedLightPs PixelShader;
    private readonly InputLayout InputLayout;
    private readonly ConstantBuffer<Constants> ConstantBuffer;
    private readonly ConstantBuffer<PerLightConstants> PerLightConstantBuffer;

    private readonly ITexture2D BrdfLut;    

    public ImageBasedLightSystem(Device device, FullScreenTriangle fullScreenTriangle, FrameService frameService, BrdfLutGenerator generator, ContentManager content)
    {
        this.Device = device;
        this.Context = device.CreateDeferredContextFor<ImageBasedLightSystem>();
        this.FullScreenTriangle = fullScreenTriangle;
        this.FrameService = frameService;

        this.VertexShader = content.LoadImageBasedLightVs();
        this.PixelShader = content.LoadImageBasedLightPs();

        this.InputLayout = this.VertexShader.CreateInputLayout(device, PostProcessVertex.Elements);
        this.ConstantBuffer = new ConstantBuffer<Constants>(device, $"{nameof(ImageBasedLightSystem)}_CB");
        this.PerLightConstantBuffer = new ConstantBuffer<PerLightConstants>(device, $"{nameof(ImageBasedLightSystem)}_per_light_CB");

        this.BrdfLut = generator.Generate();
    }  

    public void OnSet()
    {
        var blendState = this.Device.BlendStates.Additive;
        var depthStencilState = this.Device.DepthStencilStates.None;
        var width = this.FrameService.GBuffer.Width;
        var height = this.FrameService.GBuffer.Height;
        this.Context.Setup(this.InputLayout, this.VertexShader, this.PixelShader, blendState, depthStencilState, width, height);

        
        this.Context.PS.SetSampler(0, this.Device.SamplerStates.LinearClamp);
        this.Context.PS.SetShaderResource(ImageBasedLight.Albedo, this.FrameService.GBuffer.Albedo);
        this.Context.PS.SetShaderResource(ImageBasedLight.Normal, this.FrameService.GBuffer.Normal);
        this.Context.PS.SetShaderResource(ImageBasedLight.Depth, this.FrameService.GBuffer.DepthStencilBuffer);
        this.Context.PS.SetShaderResource(ImageBasedLight.Material, this.FrameService.GBuffer.Material);

        this.Context.PS.SetShaderResource(ImageBasedLight.BrdfLut, this.BrdfLut);

        this.Context.OM.SetRenderTarget(this.FrameService.LBuffer.Light);

        Matrix4x4.Invert(this.FrameService.Camera.ViewProjection, out var inverseViewProjection);
        var cameraPosition = this.FrameService.Camera.Transform.Position;
        var constants = new Constants
        {
            InverseViewProjection = inverseViewProjection,
            CameraPosition = cameraPosition
        };
        this.ConstantBuffer.MapData(this.Context, constants);
        this.Context.PS.SetConstantBuffer(Constants.Slot, this.ConstantBuffer);
    }   

    [Process(Query = ProcessQuery.All)]
    public void Render(SkyboxComponent skybox)
    {        
        var constants = new PerLightConstants
        {
            MaxReflectionLod = skybox.Environment.MipMapSlices,
            Strength = skybox.Strength,
        };
        this.PerLightConstantBuffer.MapData(this.Context, constants);
        this.Context.PS.SetConstantBuffer(PerLightConstants.Slot, this.PerLightConstantBuffer);

        this.Context.PS.SetShaderResource(ImageBasedLight.Irradiance, skybox.Irradiance);
        this.Context.PS.SetShaderResource(ImageBasedLight.Environment, skybox.Environment);

        this.Context.IA.SetVertexBuffer(this.FullScreenTriangle.Vertices);
        this.Context.IA.SetIndexBuffer(this.FullScreenTriangle.Indices);

        this.Context.DrawIndexed(FullScreenTriangle.PrimitiveCount, FullScreenTriangle.PrimitiveOffset, FullScreenTriangle.VertexOffset);
    }

    public void OnUnSet()
    {
        using var commandList = this.Context.FinishCommandList();
        this.Device.ImmediateContext.ExecuteCommandList(commandList);
    }

    public void Dispose()
    {
        this.BrdfLut.Dispose();
        this.ConstantBuffer.Dispose();
        this.InputLayout.Dispose();
        this.PixelShader.Dispose();
        this.VertexShader.Dispose();
        this.Context.Dispose();
    }
}
