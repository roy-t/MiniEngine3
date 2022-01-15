using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;

namespace Mini.Engine.UI.Panels;

[Service]
internal sealed class DevicePanel : IPanel
{
    private readonly Device Device;

    public DevicePanel(Device device)
    {
        this.Device = device;
    }

    public string Title => "Graphics Device";

    public void Update(float elapsed)
    {
        var vsync = this.Device.VSync;
        if (ImGui.Checkbox("Enable VSync", ref vsync))
        {
            this.Device.VSync = vsync;
        }
    }
}
