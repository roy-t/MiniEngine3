using Mini.Engine.Configuration;
using Mini.Engine.Content.Shaders.Generated;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Resources.Surfaces;

namespace Mini.Engine.Graphics.PostProcessing;

[Service]
public class PresentationHelper
{
    private readonly Device Device;
    private readonly FullScreenTriangle FullScreenTriangleShader;
    private readonly TextureShader TextureShader;
    private readonly ToneMapShader ToneMapShader;

    public PresentationHelper(Device device, FullScreenTriangle fullScreenTriangleShader, TextureShader textureShader, ToneMapShader toneMapShader)
    {
        this.Device = device;

        this.FullScreenTriangleShader = fullScreenTriangleShader;
        this.TextureShader = textureShader;
        this.ToneMapShader = toneMapShader;
    }

    public void Present(DeviceContext context, ISurface texture)
    {
        this.Present(context, texture, 0, 0, context.Device.Width, context.Device.Height);
    }

    public void Present(DeviceContext context, ISurface texture, int x, int y, int width, int height)
    {
        context.OM.SetRenderTargetToBackBuffer();

        context.SetupFullScreenTriangle(this.FullScreenTriangleShader.TextureVs, x, y, width, height, this.TextureShader.Ps, this.Device.BlendStates.AlphaBlend, this.Device.DepthStencilStates.None);
        context.PS.SetSampler(TextureShader.TextureSampler, this.Device.SamplerStates.LinearWrap);
        context.PS.SetShaderResource(TextureShader.Texture, texture);
        context.Draw(3);
    }

    public void ToneMapAndPresent(DeviceContext context, ISurface texture)
    {
        this.ToneMapAndPresent(context, texture, 0, 0, context.Device.Width, context.Device.Height);
    }

    public void ToneMapAndPresent(DeviceContext context, ISurface texture, int x, int y, int width, int height)
    {
        context.OM.SetRenderTargetToBackBuffer();

        context.SetupFullScreenTriangle(this.FullScreenTriangleShader.TextureVs, x, y, width, height, this.ToneMapShader.ToneMap, this.Device.BlendStates.AlphaBlend, this.Device.DepthStencilStates.None);
        context.PS.SetSampler(ToneMapShader.TextureSampler, this.Device.SamplerStates.LinearWrap);
        context.PS.SetShaderResource(ToneMapShader.Texture, texture);
        context.Draw(3);
    }
}
