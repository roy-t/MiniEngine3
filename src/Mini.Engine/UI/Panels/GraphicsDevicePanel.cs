using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.Debugging;
using Mini.Engine.DirectX;
using Mini.Engine.Titan;

namespace Mini.Engine.UI.Panels;

[Service]
internal sealed class GraphicsDevicePanel : IEditorPanel, ITitanPanel
{
    private readonly Device Device;
    private readonly RenderDoc? RenderDoc;

    private uint nextCaptureToOpen;

    public GraphicsDevicePanel(Device device, RenderDoc? renderDoc = null)
    {
        this.Device = device;

        this.RenderDoc = renderDoc;
        this.nextCaptureToOpen = uint.MaxValue;
    }

    public string Title => "Graphics Device";

    public void Update()
    {
        this.ShowRenderDoc();
        this.ShowVSync();
    }

    private void ShowRenderDoc()
    {
        if (this.RenderDoc == null)
        {
            ImGui.BeginDisabled();
        }

        if (ImGui.Button("RenderDoc Capture"))
        {
            this.nextCaptureToOpen = (this.RenderDoc?.GetNumCaptures() ?? 0) + 1;
            this.RenderDoc?.TriggerCapture();
        }

        if (this.RenderDoc != null && this.RenderDoc.GetNumCaptures() == this.nextCaptureToOpen)
        {
            var path = this.RenderDoc.GetCapture(this.RenderDoc.GetNumCaptures() - 1) ?? string.Empty;
            this.RenderDoc.LaunchReplayUI(path);
            this.nextCaptureToOpen = uint.MaxValue;
        }

        if (this.RenderDoc == null)
        {
            ImGui.EndDisabled();
        }
    }

    private void ShowVSync()
    {
        var vsync = this.Device.VSync;
        if (ImGui.Checkbox("Enable VSync", ref vsync))
        {
            this.Device.VSync = vsync;
        }
    }
}
