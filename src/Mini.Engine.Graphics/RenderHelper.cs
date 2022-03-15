using Mini.Engine.Configuration;
using Mini.Engine.Content.Shaders;
using Mini.Engine.Content.Shaders.TextureShader;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Resources;

namespace Mini.Engine.Graphics;

[Service]
public class RenderHelper
{
    private readonly Device Device;
    private readonly FullScreenTriangleTextureVs VertexShader;
    private readonly TextureShaderPs PixelShader;
    private readonly TextureShaderFxaaPs FxaaPixelShader;

    public RenderHelper(Device device, FullScreenTriangleTextureVs vertexShader, TextureShaderPs pixelShader, TextureShaderFxaaPs fxaaPixelShader)
    {
        this.Device = device;

        this.VertexShader = vertexShader;
        this.PixelShader = pixelShader;
        this.FxaaPixelShader = fxaaPixelShader;
    }

    public void Render(DeviceContext context, ITexture2D texture, int x, int y, int width, int height)
    {
        context.SetupFullScreenTriangle(this.VertexShader, x, y, width, height, this.PixelShader, this.Device.BlendStates.AlphaBlend, this.Device.DepthStencilStates.None);
        this.Render(context, texture);
    }

    public void RenderFXAA(DeviceContext context, ITexture2D texture, int x, int y, int width, int height)
    {
        context.SetupFullScreenTriangle(this.VertexShader, x, y, width, height, this.FxaaPixelShader, this.Device.BlendStates.Opaque, this.Device.DepthStencilStates.None);
        this.Render(context, texture);
    }

    private void Render(DeviceContext context, ITexture2D texture)
    {
        context.PS.SetSampler(TextureShader.TextureSampler, this.Device.SamplerStates.LinearWrap);
        context.PS.SetShaderResource(TextureShader.Texture, texture);
        context.Draw(3);
    }
}
