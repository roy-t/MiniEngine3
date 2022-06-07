using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;
using Vortice.DXGI;

namespace Mini.Engine.Graphics.Lighting;

public sealed class LightBuffer : IDisposable
{
    public LightBuffer(Device device)
{
        var imageInfo = new ImageInfo(device.Width, device.Height, Format.R16G16B16A16_Float);
        this.Light = new RenderTarget2D(device, imageInfo, nameof(LightBuffer), nameof(this.Light));
    }

    public RenderTarget2D Light { get; }

    public void Dispose()
    {
        this.Light.Dispose();
    }
}
