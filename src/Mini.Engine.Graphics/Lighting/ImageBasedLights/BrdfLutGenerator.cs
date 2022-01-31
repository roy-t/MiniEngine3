using System;
using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.Content.Shaders;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Resources;
using Vortice.Direct3D;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace Mini.Engine.Graphics.Lighting.ImageBasedLights;

[Service]
public sealed class BrdfLutGenerator : IDisposable
{
    private const int Resolution = 512;

    private readonly Device Device;
    private readonly FullScreenTriangle Triangle;
    private readonly BrdfLutGeneratorVs VertexShader;
    private readonly BrdfLutGeneratorPs PixelShader;

    private readonly InputLayout InputLayout;

    public BrdfLutGenerator(Device device, FullScreenTriangle triangle, ContentManager content)
    {
        this.Device = device;
        this.Triangle = triangle;
        this.VertexShader = content.LoadBrdfLutGeneratorVs();
        this.PixelShader = content.LoadBrdfLutGeneratorPs();

        this.InputLayout = this.VertexShader.CreateInputLayout(device, PostProcessVertex.Elements);
    }

    public ITexture2D Generate()
    {
        var context = this.Device.ImmediateContext;

        var renderTarget = new RenderTarget2D(this.Device, Resolution, Resolution, Format.R16G16_Float, "BrdfLut");
        context.OM.SetRenderTarget(renderTarget);

        this.Device.Clear(renderTarget, Color4.Black);

        context.IA.SetInputLayout(this.InputLayout);
        context.IA.SetPrimitiveTopology(PrimitiveTopology.TriangleList);

        context.VS.SetShader(this.VertexShader);

        context.RS.SetViewPort(0, 0, Resolution, Resolution);
        context.RS.SetScissorRect(0, 0, Resolution, Resolution);
        context.RS.SetRasterizerState(this.Device.RasterizerStates.CullCounterClockwise);

        context.PS.SetShader(this.PixelShader);
        context.PS.SetSampler(0, this.Device.SamplerStates.LinearWrap);

        context.OM.SetBlendState(this.Device.BlendStates.Opaque);
        context.OM.SetDepthStencilState(this.Device.DepthStencilStates.None);

        context.IA.SetVertexBuffer(this.Triangle.Vertices);
        context.IA.SetIndexBuffer(this.Triangle.Indices);
        context.DrawIndexed(FullScreenTriangle.PrimitiveCount, FullScreenTriangle.PrimitiveOffset, FullScreenTriangle.VertexOffset);

        return renderTarget;
    }

    public void Dispose()
    {
        this.InputLayout.Dispose();
    }
}
