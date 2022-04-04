using Mini.Engine.Configuration;
using Mini.Engine.Content.Shaders;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace Mini.Engine.Graphics.Lighting.ImageBasedLights;

[Service]
public sealed class BrdfLutGenerator
{
    private const int Resolution = 512;

    private readonly Device Device;
    private readonly FullScreenTriangleTextureVs VertexShader;
    private readonly BrdfLutGeneratorPs PixelShader;

    public BrdfLutGenerator(Device device, FullScreenTriangleTextureVs vertexShader, BrdfLutGeneratorPs pixelShader)
    {
        this.Device = device;
        this.VertexShader = vertexShader;
        this.PixelShader = pixelShader;
    }

    public ITexture2D Generate()
    {
        var context = this.Device.ImmediateContext;

        var renderTarget = new RenderTarget2D(this.Device, Resolution, Resolution, Format.R16G16_Float, "BrdfLut");

        context.SetupFullScreenTriangle(this.VertexShader, Resolution, Resolution, this.PixelShader, this.Device.BlendStates.Opaque, this.Device.DepthStencilStates.None);
        context.OM.SetRenderTarget(renderTarget);

        context.Clear(renderTarget, Colors.Black);
        context.PS.SetSampler(0, this.Device.SamplerStates.LinearWrap);
        context.Draw(3);

        return renderTarget;
    }
}
