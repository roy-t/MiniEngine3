using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using Mini.Engine.Debugging;
using Mini.Engine.UI.Menus;
using Mini.Engine.UI.Panels;

namespace Mini.Engine.UI;

internal abstract class UserInterface
{
    private sealed record MenuRegistration(string Title, IMenu Menu)
    {
        public void Update()
        {
            this.Menu.Update();
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

        public void Update()
        {
            ImGui.PushID(this.Id);
            this.Panel.Update();
            ImGui.PopID();
        }
    }

    private readonly UICore Core;
    private readonly MetricService Metrics;
    private readonly List<PanelRegistration> Panels;
    private readonly List<MenuRegistration> Menus;

    private readonly Stopwatch Stopwatch;

    public UserInterface(UICore core, MetricService metrics, IEnumerable<IPanel> panels, IEnumerable<IMenu> menus)
    {
        this.Core = core;
        this.Metrics = metrics;
        this.Panels = panels.Select(p => new PanelRegistration(p.Title, p.Title, p, true)).ToList();
        this.Menus = menus.Select(m => new MenuRegistration(m.Title, m)).ToList();

        this.Stopwatch = new Stopwatch();
    }

    public void Resize(int width, int height)
    {
        this.Core.Resize(width, height);
    }

    public void NewFrame()
    {
        this.Stopwatch.Restart();

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
                    menu.Update();

                    ImGui.EndMenu();
                }
            }

            this.DrawPerfWidget();
            ImGui.EndMainMenuBar();
        }

        foreach (var panel in this.Panels)
        {
            if (panel.IsVisible)
            {
                var isVisible = true;
                if (ImGui.Begin(panel.Title, ref isVisible))
                {
                    panel.Update();
                    ImGui.End();
                }

                panel.IsVisible = isVisible;
            }
        }

        this.Metrics.Update("UserInterface.Run.Millis", (float)this.Stopwatch.Elapsed.TotalMilliseconds);
    }

    private void DrawPerfWidget()
    {
        var max = 0.0f;
        foreach (var gauge in this.Metrics.Gauges)
        {
            if (gauge.Tag == "GameManager.Run.Millis")
            {
                max = gauge.Max;
                break;
            }
        }

        var color = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
        if (max > 15.0f)
        {
            color = new Vector4(1.0f, 0.15f, 0.0f, 1.0f);
        }
        if (max > 16.67f)
        {
            color = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
        }

        var text = $"{max:F2} ms";
        var width = ImGui.CalcTextSize(text).X;

        ImGui.SameLine(ImGui.GetWindowWidth() - width - 15);
        ImGui.TextColored(color, text);
    }
}
