using Mini.Engine.Configuration;
using Mini.Engine.Content.Shaders.Generated;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.ECS.Systems;

namespace Mini.Engine.Graphics.PostProcessing;

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

    public void OnSet()
    {
        var blend = this.Device.BlendStates.Opaque;
        var depth = this.Device.DepthStencilStates.None;
        this.Context.SetupFullScreenTriangle(this.FullScreenTriangleShader.TextureVs, this.Shader.FxaaPs, blend, depth);
        this.Context.PS.SetSampler(AntiAliasShader.TextureSampler, this.Device.SamplerStates.LinearWrap);
        this.Context.PS.SetShaderResource(AntiAliasShader.Texture, this.FrameService.LBuffer.Light);

        this.FrameService.PBuffer.Swap();
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
