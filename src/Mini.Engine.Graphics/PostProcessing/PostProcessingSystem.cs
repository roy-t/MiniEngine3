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
        this.FrameService.PBuffer.Swap();

        var blend = this.Device.BlendStates.Opaque;
        var depth = this.Device.DepthStencilStates.None;

        var shader = this.FrameService.PBuffer.AntiAliasing switch
        {
            AAType.None => this.Shader.NonePs,
            AAType.FXAA => this.Shader.FxaaPs,
            AAType.TAA => this.Shader.TaaPs,
            _ => throw new NotImplementedException()
        };

        this.Context.SetupFullScreenTriangle(this.FullScreenTriangleShader.TextureVs, shader, blend, depth);

        this.Context.PS.SetSampler(AntiAliasShader.TextureSampler, this.Device.SamplerStates.LinearClamp);
        this.Context.PS.SetShaderResource(AntiAliasShader.Color, this.FrameService.LBuffer.Light);
        this.Context.PS.SetShaderResource(AntiAliasShader.PreviousColor, this.FrameService.PBuffer.PreviousColor);
        this.Context.PS.SetShaderResource(AntiAliasShader.Velocity, this.FrameService.GBuffer.Velocity);
        this.Context.PS.SetShaderResource(AntiAliasShader.PreviousVelocity, this.FrameService.PBuffer.PreviousVelocity);

        this.Context.OM.SetRenderTargets(null, this.FrameService.PBuffer.CurrentColor, this.FrameService.PBuffer.CurrentVelocity);
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
