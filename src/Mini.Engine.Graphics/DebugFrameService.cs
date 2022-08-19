using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Surfaces;
using Vortice.DXGI;

namespace Mini.Engine.Graphics;

[Service]
public sealed class DebugFrameService
{
    public DebugFrameService(Device device)
    {
        var imageInfo = new ImageInfo(device.Width, device.Height, Format.R8G8B8A8_UNorm_SRgb);
        this.DebugOverlay = new RenderTarget(device, nameof(this.DebugOverlay), imageInfo, MipMapInfo.None());
#if DEBUG
        this.EnableDebugOverlay = true;
        this.RenderToViewport = true;
#endif
        this.ShowBounds = false;
    }

    public IRenderTarget DebugOverlay { get; }

    public bool EnableDebugOverlay { get; set; }
    public bool ShowBounds { get; set; }
    public bool RenderToViewport { get; set; }
}
