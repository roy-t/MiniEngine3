using Mini.Engine.Configuration;
using Mini.Engine.Content.Shaders.Generated;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.ECS.Systems;

namespace Mini.Engine.Graphics.PostProcessing;

public enum AAType { None, FXAA, TAA };

[Service]
public sealed partial class PostProcessingSystem : ISystem, IDisposable
{
    private readonly Device Device;
    private readonly DeferredDeviceContext Context;
    private readonly FrameService FrameService;
    private readonly FullScreenTriangle FullScreenTriangleShader;
    private readonly AntiAliasShader Shader;

    public PostProcessingSystem(Device device, FrameService frameService, FullScreenTriangle fullScreenTriangleShader, AntiAliasShader shader)
    {
        this.Device = device;
        this.Context = device.CreateDeferredContextFor<PostProcessingSystem>();
        this.FrameService = frameService;
        this.FullScreenTriangleShader = fullScreenTriangleShader;
        this.Shader = shader;
    }

    public AAType AntiAliasing { get; set; } = AAType.None;

    public void OnSet()
    {
        this.FrameService.PBuffer.Swap();

        var blend = this.Device.BlendStates.Opaque;
        var depth = this.Device.DepthStencilStates.None;

        var shader = this.AntiAliasing switch
        {
            AAType.None => this.Shader.NonePs,
            AAType.FXAA => this.Shader.FxaaPs,
            AAType.TAA => this.Shader.TaaPs,
            _ => throw new NotImplementedException()
        };
        
        this.Context.SetupFullScreenTriangle(this.FullScreenTriangleShader.TextureVs, shader, blend, depth);

        this.Context.PS.SetSampler(AntiAliasShader.TextureSampler, this.Device.SamplerStates.LinearWrap);
        this.Context.PS.SetShaderResource(AntiAliasShader.PreviousTexture, this.FrameService.PBuffer.Previous);
        this.Context.PS.SetShaderResource(AntiAliasShader.Texture, this.FrameService.LBuffer.Light);
        
        this.Context.OM.SetRenderTarget(this.FrameService.PBuffer.Current);
    }

    [Process(Query = ProcessQuery.None)]
    public void PostProcess()
    {
        this.Context.Draw(3);
    }

    public void OnUnSet()
    {
        using var commandList = this.Context.FinishCommandList();
        this.Device.ImmediateContext.ExecuteCommandList(commandList);
    }

    public void Dispose()
    {
        this.Context.Dispose();
    }
}
