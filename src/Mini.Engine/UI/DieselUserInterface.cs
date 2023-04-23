using Mini.Engine.Configuration;
using Mini.Engine.Debugging;
using Mini.Engine.UI.Menus;
using Mini.Engine.UI.Panels;

namespace Mini.Engine.UI;

[Service]
internal sealed class DieselUserInterface : UserInterface
{
    public DieselUserInterface(MetricService metrics, UICore core, IEnumerable<IDieselPanel> panels, IEnumerable<IDieselMenu> menus)
        : base(core, metrics, panels, menus) { }
}
