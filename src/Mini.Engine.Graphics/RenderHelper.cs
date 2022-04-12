using Mini.Engine.Configuration;
using Mini.Engine.Content.Shaders.Generated;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Resources;

namespace Mini.Engine.Graphics;

[Service]
public class RenderHelper
{
    private readonly Device Device;
    private readonly FullScreenTriangle FullScreenTriangleShader;
    private readonly TextureShader TextureShader;    

    public RenderHelper(Device device, FullScreenTriangle fullScreenTriangleShader, TextureShader textureShader)
    {
        this.Device = device;

        this.FullScreenTriangleShader = fullScreenTriangleShader;
        this.TextureShader = textureShader;
    }

    public void Render(DeviceContext context, ITexture2D texture, int x, int y, int width, int height)
    {
        context.SetupFullScreenTriangle(this.FullScreenTriangleShader.TextureVs, x, y, width, height, this.TextureShader.Ps, this.Device.BlendStates.AlphaBlend, this.Device.DepthStencilStates.None);
        this.Render(context, texture);
    }

    public void RenderFXAA(DeviceContext context, ITexture2D texture, int x, int y, int width, int height)
    {
        context.SetupFullScreenTriangle(this.FullScreenTriangleShader.TextureVs, x, y, width, height, this.TextureShader.FxaaPs, this.Device.BlendStates.Opaque, this.Device.DepthStencilStates.None);
        this.Render(context, texture);
    }

    private void Render(DeviceContext context, ITexture2D texture)
    {
        context.PS.SetSampler(TextureShader.TextureSampler, this.Device.SamplerStates.LinearWrap);
        context.PS.SetShaderResource(TextureShader.Texture, texture);
        context.Draw(3);
    }
}
