using System.Drawing;
using Mini.Engine.Configuration;
using Mini.Engine.Content.Shaders.Generated;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Contexts.States;
using Mini.Engine.ECS.Systems;

namespace Mini.Engine.Graphics.PostProcessing;

[Service]
public sealed class PostProcessingSystem : IDisposable
{
    private readonly DeferredDeviceContext Context;
    private readonly ImmediateDeviceContext CompletionContext;
    private readonly FrameService FrameService;
    private readonly FullScreenTriangle FullScreenTriangleShader;
    private readonly AntiAliasShader Shader;

    private readonly BlendState Opaque;
    private readonly DepthStencilState None;
    private readonly SamplerState LinearClamp;

    public PostProcessingSystem(Device device, FrameService frameService, FullScreenTriangle fullScreenTriangleShader, AntiAliasShader shader)
    {
        this.Context = device.CreateDeferredContextFor<PostProcessingSystem>();
        this.CompletionContext = device.ImmediateContext;
        this.FrameService = frameService;
        this.FullScreenTriangleShader = fullScreenTriangleShader;
        this.Shader = shader;

        this.Opaque = device.BlendStates.Opaque;
        this.None = device.DepthStencilStates.None;
        this.LinearClamp = device.SamplerStates.LinearClamp;
    }

    public Task<ICompletable> Render(Rectangle viewport, Rectangle scissor)
    {
        return Task.Run(() =>
        {
            this.Setup(viewport, scissor);
            this.PostProcess();
            return CompletableCommandList.Create(this.CompletionContext, this.Context.FinishCommandList());
        });
    }

    private void Setup(in Rectangle viewport, in Rectangle scissor)
    {
        ref var camera = ref this.FrameService.GetPrimaryCamera();

        var shader = this.FrameService.PBuffer.AntiAliasing switch
        {
            AAType.None => this.Shader.NonePs,
            AAType.FXAA => this.Shader.FxaaPs,
            AAType.TAA => this.Shader.TaaPs,
            _ => throw new NotImplementedException()
        };

        this.Context.SetupFullScreenTriangle(this.FullScreenTriangleShader.TextureVs, in viewport, in scissor, shader, this.Opaque, this.None);

        this.Context.PS.SetSampler(AntiAliasShader.TextureSampler, this.LinearClamp);
        this.Context.PS.SetShaderResource(AntiAliasShader.Color, this.FrameService.LBuffer.Light);
        this.Context.PS.SetShaderResource(AntiAliasShader.PreviousColor, this.FrameService.PBuffer.PreviousColor);
        this.Context.PS.SetShaderResource(AntiAliasShader.Velocity, this.FrameService.GBuffer.Velocity);
        this.Context.PS.SetShaderResource(AntiAliasShader.PreviousVelocity, this.FrameService.PBuffer.PreviousVelocity);

        this.Context.OM.SetRenderTarget(this.FrameService.PBuffer.CurrentColor);
    }

    private void PostProcess()
    {
        this.Context.Draw(3);
    }

    public void Dispose()
    {
        this.Context.Dispose();
    }
}
