using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Surfaces;
using Vortice.DXGI;

namespace Mini.Engine.Graphics.Lighting;

public sealed class LightBuffer : IDisposable
{
    public LightBuffer(Device device)
{
        var imageInfo = new ImageInfo(device.Width, device.Height, Format.R16G16B16A16_Float);
        this.Light = new RenderTarget(device, nameof(LightBuffer) + nameof(this.Light), imageInfo, MipMapInfo.None());
        this.Group = new RenderTargetGroup(this.Light);
    }

    public IRenderTarget Light { get; }
    public RenderTargetGroup Group { get; }

    public void Dispose()
    {
        this.Group.Dispose();
        this.Light.Dispose();
    }
}
