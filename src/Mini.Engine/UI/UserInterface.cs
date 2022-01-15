using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.DirectX;
using Mini.Engine.UI.Panels;
using Mini.Engine.Windows;

namespace Mini.Engine.UI;

[Service]
public sealed class UserInterface : IDisposable
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

    private readonly ImGuiRenderer ImGuiRenderer;
    private readonly ImGuiInputHandler ImguiInputHandler;
    private readonly ImGuiIOPtr IO;
    private readonly MicroBenchmark MicroBenchmark;

    private readonly List<PanelRecord> Panels;

    public UserInterface(Win32Window window, Device device, ContentManager content, UITextureRegistry textureRegistry, IEnumerable<IPanel> panels)
    {
        _ = ImGui.CreateContext();
        this.IO = ImGui.GetIO();
        this.ImGuiRenderer = new ImGuiRenderer(device, content, textureRegistry);
        this.ImguiInputHandler = new ImGuiInputHandler(window.Handle);
        this.MicroBenchmark = new MicroBenchmark("Perf");

        this.Panels = panels.Select(p => new PanelRecord(p.Title, p, true)).ToList();

        this.Resize(window.Width, window.Height);
    }

    public void Resize(int width, int height)
    {
        this.IO.DisplaySize = new Vector2(width, height);
    }

    public void NewFrame(float elapsed)
    {
        this.IO.DeltaTime = elapsed;

        this.ImguiInputHandler.Update();
        this.MicroBenchmark.Update(elapsed);

        ImGui.NewFrame();
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
        ImGui.Render();
        this.ImGuiRenderer.Render(ImGui.GetDrawData());
    }

    public void Dispose()
    {
        this.ImGuiRenderer.Dispose();
    }
}
