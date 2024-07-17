using Mini.Engine.Configuration;
using Mini.Engine.Debugging;
using Mini.Engine.UI;
using Mini.Engine.UI.Menus;
using Mini.Engine.UI.Panels;

namespace Mini.Engine.Titan;

public interface ITitanPanel : IPanel
{

}

public interface ITitanMenu : IMenu
{

}

[Service]
internal sealed class TitanUserInterface : UserInterface
{
    public TitanUserInterface(UICore ui, MetricService metrics, IEnumerable<ITitanPanel> panels, IEnumerable<ITitanMenu> menus)
        : base(ui, metrics, panels, menus)
    {

    }
}
