using System;
using Mini.Engine.Configuration;
using Mini.Engine.Content.Shaders;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Resources;
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

    public BrdfLutGenerator(Device device, FullScreenTriangle triangle, BrdfLutGeneratorVs vertexShader, BrdfLutGeneratorPs pixelShader)
    {
        this.Device = device;
        this.Triangle = triangle;
        this.VertexShader = vertexShader;
        this.PixelShader = pixelShader;

        this.InputLayout = this.VertexShader.CreateInputLayout(device, PostProcessVertex.Elements);
    }

    public ITexture2D Generate()
    {
        var context = this.Device.ImmediateContext;

        var renderTarget = new RenderTarget2D(this.Device, Resolution, Resolution, Format.R16G16_Float, "BrdfLut");

        context.Setup(this.InputLayout, this.VertexShader, this.PixelShader, this.Device.BlendStates.Opaque, this.Device.DepthStencilStates.None, Resolution, Resolution);
        context.OM.SetRenderTarget(renderTarget);

        this.Device.Clear(renderTarget, Color4.Black);        
        context.PS.SetSampler(0, this.Device.SamplerStates.LinearWrap);        
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
