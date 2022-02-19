using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.Content.Shaders;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.Content.Shaders.TextureShader;
using Vortice.Direct3D;

namespace Mini.Engine.Graphics;

[Service]
public class RenderHelper
{
    private readonly Device Device;    
    private readonly TextureShaderVs VertexShader;
    private readonly TextureShaderFxaaPs FxaaPixelShader;
    private readonly TextureShaderPs UnmodifiedPixelShader;

    private readonly FullScreenTriangle FullScreenTriangle;
    private readonly InputLayout InputLayout;

    public RenderHelper(Device device, FullScreenTriangle fullScreenTriangle, ContentManager content)
    {
        this.Device = device;
        this.FullScreenTriangle = fullScreenTriangle;
        
        this.VertexShader = content.LoadTextureShaderVs();
        this.FxaaPixelShader = content.LoadTextureShaderFxaaPs();
        this.UnmodifiedPixelShader = content.LoadTextureShaderPs();

        this.InputLayout = this.VertexShader.CreateInputLayout(device, PostProcessVertex.Elements);
    }
    
    public void RenderFXAA(DeviceContext context, ITexture2D texture, int x, int y, int width, int height)
    {
        context.Setup(this.InputLayout, PrimitiveTopology.TriangleList, this.VertexShader, this.Device.RasterizerStates.CullCounterClockwise, x, y, width, height, this.FxaaPixelShader, this.Device.BlendStates.Opaque, this.Device.DepthStencilStates.None);
        this.Render(context, texture);
    }

    public void Render(DeviceContext context, ITexture2D texture, int x, int y, int width, int height)
    {
        context.Setup(this.InputLayout, PrimitiveTopology.TriangleList, this.VertexShader, this.Device.RasterizerStates.CullCounterClockwise, x, y, width, height, this.UnmodifiedPixelShader, this.Device.BlendStates.AlphaBlend, this.Device.DepthStencilStates.None);
        this.Render(context, texture);
    }

    private void Render(DeviceContext context, ITexture2D texture)
    {
        context.PS.SetSampler(TextureShader.TextureSampler, this.Device.SamplerStates.LinearWrap);

        context.IA.SetVertexBuffer(this.FullScreenTriangle.Vertices);
        context.IA.SetIndexBuffer(this.FullScreenTriangle.Indices);
     
        context.PS.SetShaderResource(TextureShader.Texture, texture);
        context.DrawIndexed(FullScreenTriangle.PrimitiveCount, FullScreenTriangle.PrimitiveOffset, FullScreenTriangle.VertexOffset);
    }
}
