using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;

namespace Mini.Engine.UI.Menus;

[Service]
internal class DeviceMenu : IEditorMenu, IDieselMenu
{
    private readonly Device Device;

    public DeviceMenu(Device device)
    {
        this.Device = device;
    }

    public string Title => "Device";

    public void Update(float elapsed)
    {
        var vSync = this.Device.VSync;
        if (ImGui.Checkbox("Vertical Sync", ref vSync))
        {
            this.Device.VSync = vSync;
        }
    }
}
