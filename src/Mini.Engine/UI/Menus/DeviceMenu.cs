using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.Debugging;
using Mini.Engine.DirectX;

namespace Mini.Engine.UI.Menus;

[Service]
internal class DeviceMenu : IEditorMenu
{
    private readonly Device Device;
    private readonly RenderDoc? RenderDoc;

    private uint nextCaptureToOpen;

    public DeviceMenu(Device device, RenderDoc? renderDoc = null)
    {
        this.Device = device;
        this.RenderDoc = renderDoc;
        this.nextCaptureToOpen = uint.MaxValue;
    }

    public string Title => "Device";

    public void Update()
    {
        var vSync = this.Device.VSync;
        if (ImGui.Checkbox("Vertical Sync", ref vSync))
        {
            this.Device.VSync = vSync;
        }

        this.ShowRenderDoc();
    }

    private void ShowRenderDoc()
    {
        if (this.RenderDoc == null)
        {
            ImGui.BeginDisabled(true);
            ImGui.MenuItem("Capture Frame");
            ImGui.EndDisabled();
        }
        else
        {
            if (ImGui.MenuItem("Capture Frame"))
            {
                this.nextCaptureToOpen = this.RenderDoc.GetNumCaptures() + 1;
                this.RenderDoc.TriggerCapture();
            }

            if (this.RenderDoc.GetNumCaptures() == this.nextCaptureToOpen)
            {
                var path = this.RenderDoc.GetCapture(this.RenderDoc.GetNumCaptures() - 1) ?? string.Empty;
                this.RenderDoc.LaunchReplayUI(path);
                this.nextCaptureToOpen = uint.MaxValue;

            }
        }
    }
}
