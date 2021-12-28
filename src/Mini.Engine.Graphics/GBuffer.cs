using Mini.Engine.DirectX;
using Vortice.DXGI;

namespace Mini.Engine.Graphics;

public sealed class GBuffer
{
    public GBuffer(Device device, DepthStencilFormat depthStencilFormat)
    {
        this.Albedo = new RenderTarget2D(device, device.Width, device.Height, Format.B8G8R8A8_UNorm_SRgb, "GBuffer_Albedo");
        this.DepthStencilBuffer = new DepthStencilBuffer(device, depthStencilFormat, device.Width, device.Height);

        this.Width = device.Width;
        this.Height = device.Height;
    }

    public int Width { get; }
    public int Height { get; }

    public float AspectRatio => (float)this.Width / (float)this.Height;

    public RenderTarget2D Albedo { get; }
    public DepthStencilBuffer DepthStencilBuffer { get; }
}
