using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.Graphics;

namespace Mini.Engine.UI.Panels;

[Service]
internal sealed class DebugPanel : IPanel
{
    private readonly DebugFrameService FrameService;

    public DebugPanel(DebugFrameService frameService)
    {
        this.FrameService = frameService;
    }

    public string Title => "Debug";

    public void Update(float elapsed)
    {
        var enableDebugOverlay = this.FrameService.EnableDebugOverlay;
        if (ImGui.Checkbox("Enable Debug Overlay", ref enableDebugOverlay))
        {
            this.FrameService.EnableDebugOverlay = enableDebugOverlay;
        }

        var showBounds = this.FrameService.ShowBounds;
        if (ImGui.Checkbox("Show Bounds", ref showBounds))
        {
            this.FrameService.ShowBounds = showBounds;
        }

        var renderToViewPort = this.FrameService.RenderToViewport;
        if (ImGui.Checkbox("Render To Viewport", ref renderToViewPort))
        {
            this.FrameService.RenderToViewport = renderToViewPort;
        }
    }
}
