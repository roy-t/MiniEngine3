﻿using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.Content.Shaders;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.Content.Shaders.PostProcess;
using Vortice.Direct3D;
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
        this.Render(context, texture);
    }

    public void RenderToRenderTarget(DeviceContext context, RenderTarget2D renderTarget, ITexture2D texture)
    {
        context.OM.SetRenderTarget(renderTarget);
        this.Render(context, texture);
    }

    private void Render(DeviceContext context, ITexture2D texture)
    {
        var width = this.Device.Width;
        var height = this.Device.Height;

        context.IA.SetInputLayout(this.InputLayout);
        context.IA.SetPrimitiveTopology(PrimitiveTopology.TriangleList);

        context.VS.SetShader(this.VertexShader);

        context.RS.SetViewPort(0, 0, width, height);
        context.RS.SetScissorRect(0, 0, width, height);
        context.RS.SetRasterizerState(this.Device.RasterizerStates.CullCounterClockwise);

        context.PS.SetShader(this.PixelShader);
        context.PS.SetSampler(PostProcess.Texture, this.Device.SamplerStates.LinearWrap);


        context.OM.SetBlendState(this.Device.BlendStates.Opaque);
        context.OM.SetDepthStencilState(this.Device.DepthStencilStates.None);

        context.IA.SetVertexBuffer(this.FullScreenTriangle.Vertices);
        context.IA.SetIndexBuffer(this.FullScreenTriangle.Indices);
        context.PS.SetShaderResource(0, texture);
        context.DrawIndexed(FullScreenTriangle.PrimitiveCount, FullScreenTriangle.PrimitiveOffset, FullScreenTriangle.VertexOffset);
    }
}
