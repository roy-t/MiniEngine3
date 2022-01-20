using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.Content.Shaders;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.Content.Shaders.PostProcess;
using Vortice.Direct3D;
using System.Numerics;

namespace Mini.Engine.Graphics;

[Service]
public class RenderHelper
{
    private readonly Device Device;
    private readonly PostProcessPs PixelShader;
    private readonly PostProcessVs VertexShader;
    private readonly FullScreenTriangle FullScreenTriangle;
    private readonly InputLayout InputLayout;

    public RenderHelper(Device device, FullScreenTriangle fullScreenTriangle, ContentManager content)
    {
        this.Device = device;
        this.FullScreenTriangle = fullScreenTriangle;
        this.PixelShader = content.LoadPostProcessPs();
        this.VertexShader = content.LoadPostProcessVs();

        this.InputLayout = this.VertexShader.CreateInputLayout(device, PostProcessVertex.Elements);
    }

    public void RenderToViewPort(DeviceContext context, ITexture2D texture)
    {
        context.OM.SetRenderTargetToBackBuffer();
        this.Render(context, texture, 0, 0, this.Device.Width, this.Device.Height);
    }

    public void RenderToViewPort(DeviceContext context, ITexture2D texture, int x, int y, int width, int height)
    {
        context.OM.SetRenderTargetToBackBuffer();
        this.Render(context, texture, x, y, width, height);
    }

    private void Render(DeviceContext context, ITexture2D texture, int x, int y, int width, int height)
    {
        context.IA.SetInputLayout(this.InputLayout);
        context.IA.SetPrimitiveTopology(PrimitiveTopology.TriangleList);

        context.VS.SetShader(this.VertexShader);

        context.RS.SetViewPort(x, y, width, height);
        context.RS.SetScissorRect(x, y, width, height);
        context.RS.SetRasterizerState(this.Device.RasterizerStates.CullCounterClockwise);

        context.PS.SetShader(this.PixelShader);
        context.PS.SetSampler(PostProcess.TextureSampler, this.Device.SamplerStates.LinearWrap);


        context.OM.SetBlendState(this.Device.BlendStates.Opaque);
        context.OM.SetDepthStencilState(this.Device.DepthStencilStates.None);

        context.IA.SetVertexBuffer(this.FullScreenTriangle.Vertices);
        context.IA.SetIndexBuffer(this.FullScreenTriangle.Indices);
        context.PS.SetShaderResource(PostProcess.Texture, texture);
        context.DrawIndexed(FullScreenTriangle.PrimitiveCount, FullScreenTriangle.PrimitiveOffset, FullScreenTriangle.VertexOffset);
    }
}
