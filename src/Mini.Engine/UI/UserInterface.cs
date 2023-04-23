using ImGuiNET;
using Mini.Engine.UI.Menus;
using Mini.Engine.UI.Panels;

namespace Mini.Engine.UI;

internal abstract class UserInterface
{
    private sealed record MenuRegistration(string Title, IMenu Menu)
    {
        public void Update(float elapsed)
        {
            this.Menu.Update(elapsed);
        }
    }

    private sealed class PanelRegistration
    {
        public PanelRegistration(string id, string title, IPanel panel, bool isVisible)
        {
            this.Id = id;
            this.Title = title;
            this.Panel = panel;
            this.IsVisible = isVisible;
        }

        public string Id { get; }
        public string Title { get; }
        public IPanel Panel { get; }
        public bool IsVisible { get; set; }

        public void Update(float elapsed)
        {
            ImGui.PushID(this.Id);
            this.Panel.Update(elapsed);
            ImGui.PopID();
        }
    }

    private readonly UICore Core;
    private readonly MicroBenchmark MicroBenchmark;
    private readonly List<PanelRegistration> Panels;
    private readonly List<MenuRegistration> Menus;

    public UserInterface(UICore core, IEnumerable<IPanel> panels, IEnumerable<IMenu> menus)
    {
        this.Core = core;
        this.MicroBenchmark = new MicroBenchmark("Perf");
        this.Panels = panels.Select(p => new PanelRegistration(p.Title, p.Title, p, true)).ToList();
        this.Menus = menus.Select(m => new MenuRegistration(m.Title, m)).ToList();
    }

    public void Resize(int width, int height)
    {
        this.Core.Resize(width, height);
    }

    public void NewFrame(float elapsed)
    {
        this.Core.NewFrame(elapsed);
        this.MicroBenchmark.Update(elapsed);

        ImGui.DockSpaceOverViewport(ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);

        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("View"))
            {
                foreach (var panel in this.Panels)
                {
                    var isVisible = panel.IsVisible;
                    if (ImGui.Checkbox(panel.Title, ref isVisible))
                    {
                        panel.IsVisible = isVisible;
                    }
                }

                ImGui.EndMenu();
            }

            foreach (var menu in this.Menus)
            {
                if (ImGui.BeginMenu(menu.Title))
                {
                    menu.Update(elapsed);

                    ImGui.EndMenu();
                }
            }

            ImGui.Text(this.MicroBenchmark.ToString());
            ImGui.EndMainMenuBar();
        }

        foreach (var panel in this.Panels)
        {
            if (panel.IsVisible)
            {
                var isVisible = true;
                if (ImGui.Begin(panel.Title, ref isVisible))
                {
                    panel.Update(elapsed);
                    ImGui.End();
                }

                panel.IsVisible = isVisible;
            }
        }
    }

    public void Render()
    {
        this.Core.Render();
    }
}
