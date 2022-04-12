using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;
using Vortice.DXGI;

namespace Mini.Engine.Graphics;

[Service]
public sealed class DebugFrameService
{
    public DebugFrameService(Device device)
    {
        this.DebugOverlay = new RenderTarget2D(device, device.Width, device.Height, Format.R8G8B8A8_UNorm_SRgb, nameof(DebugFrameService), nameof(this.DebugOverlay));
#if DEBUG
        this.EnableDebugOverlay = true;
        this.RenderToViewport = true;
#endif
        this.ShowBounds = false;
    }

    public RenderTarget2D DebugOverlay { get; }

    public bool EnableDebugOverlay { get; set; }
    public bool ShowBounds { get; set; }
    public bool RenderToViewport { get; set; }
}
