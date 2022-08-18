using Mini.Engine.Configuration;
using Mini.Engine.Content.Shaders.Generated;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.DirectX.Resources.vNext;
using Vortice.DXGI;
using Vortice.Mathematics;
using Shaders = Mini.Engine.Content.Shaders.Generated;

namespace Mini.Engine.Graphics.Lighting.ImageBasedLights;

[Service]
public sealed class BrdfLutGenerator
{
    private const int Resolution = 512;

    private readonly Device Device;
    private readonly FullScreenTriangle FullScreenTriangle;
    private readonly Shaders.BrdfLutGenerator Shader;

    public BrdfLutGenerator(Device device, FullScreenTriangle fullScreenTriangle, Shaders.BrdfLutGenerator shader)
    {        
        this.Device = device;
        this.FullScreenTriangle = fullScreenTriangle;
        this.Shader = shader;        
    }

    public ISurface Generate(string user)
    {
        var context = this.Device.ImmediateContext;

        var  imageInfo = new ImageInfo(Resolution, Resolution, Format.R16G16_Float);
        var renderTarget = new RenderTarget2D(this.Device, imageInfo, user, "BrdfLut");

        context.SetupFullScreenTriangle(this.FullScreenTriangle.TextureVs, Resolution, Resolution, this.Shader.Ps, this.Device.BlendStates.Opaque, this.Device.DepthStencilStates.None);
        context.OM.SetRenderTarget(renderTarget);

        context.Clear(renderTarget, Colors.Black);
        context.PS.SetSampler(0, this.Device.SamplerStates.LinearWrap);
        context.Draw(3);

        return renderTarget;
    }
}
