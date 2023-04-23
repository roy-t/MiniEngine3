using Mini.Engine.Configuration;
using Mini.Engine.Debugging;
using Mini.Engine.UI.Menus;
using Mini.Engine.UI.Panels;

namespace Mini.Engine.UI;

[Service]
internal sealed class EditorUserInterface : UserInterface
{
    public EditorUserInterface(MetricService metrics, UICore core, IEnumerable<IEditorPanel> panels, IEnumerable<IEditorMenu> menus)
        : base(core, metrics, panels, menus) { }
}
