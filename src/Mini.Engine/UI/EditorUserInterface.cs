﻿using System.Collections.Generic;
using System.Linq;
using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.UI.Panels;

namespace Mini.Engine.UI;

[Service]
public sealed class EditorUserInterface
{
    private sealed class PanelRecord
    {
        public PanelRecord(string title, IPanel panel, bool isVisible)
        {
            this.Title = title;
            this.Panel = panel;
            this.IsVisible = isVisible;
        }

        public string Title { get; }
        public IPanel Panel { get; }
        public bool IsVisible { get; set; }

        public void Update(float elapsed)
        {
            this.Panel.Update(elapsed);
        }
    }

    private readonly UICore Core;
    private readonly MicroBenchmark MicroBenchmark;
    private readonly List<PanelRecord> Panels;

    public EditorUserInterface(UICore core, IEnumerable<IPanel> panels)
    {
        this.Core = core;
        this.MicroBenchmark = new MicroBenchmark("Perf");
        this.Panels = panels.Select(p => new PanelRecord(p.Title, p, true)).ToList();
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
