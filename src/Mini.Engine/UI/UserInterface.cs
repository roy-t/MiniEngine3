using System;
using System.Numerics;
using ImGuiNET;
using Mini.Engine.Debugging;
using Mini.Engine.DirectX;

namespace Mini.Engine.UI
{
    internal sealed class UserInterface : IDisposable
    {
        private readonly ImGuiRenderer ImGuiRenderer;
        private readonly ImGuiInputHandler ImguiInputHandler;
        private readonly RenderDoc RenderDoc;
        private readonly ImGuiIOPtr IO;
        private readonly MicroBenchmark MicroBenchmark;


        public UserInterface(RenderDoc renderDoc, Device device, IntPtr windowHandle, int width, int height)
        {
            this.RenderDoc = renderDoc;

            _ = ImGui.CreateContext();
            this.IO = ImGui.GetIO();
            this.ImGuiRenderer = new ImGuiRenderer(device);
            this.ImguiInputHandler = new ImGuiInputHandler(windowHandle);
            this.MicroBenchmark = new MicroBenchmark("MicroBenchmark", TimeSpan.FromSeconds(5));

            this.Resize(width, height);
        }

        public void Resize(int width, int height)
            => this.IO.DisplaySize = new Vector2(width, height);

        public void Update(float elapsed)
        {
            this.IO.DeltaTime = elapsed;

            this.ImguiInputHandler.Update();
            this.MicroBenchmark.Update();

            ImGui.NewFrame();
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("RenderDoc"))
                {

                    if (ImGui.MenuItem("Launch Replay UI"))
                    {
                        _ = this.RenderDoc.LaunchReplayUI();
                    }

                    if (ImGui.MenuItem("Capture"))
                    {
                        this.RenderDoc.TriggerCapture();
                    }

                    if (ImGui.MenuItem("Open Last Capture", this.RenderDoc.GetNumCaptures() > 0))
                    {
                        var path = this.RenderDoc.GetCapture(this.RenderDoc.GetNumCaptures() - 1);
                        _ = this.RenderDoc.LaunchReplayUI(path);
                    }

                    ImGui.EndMenu();
                }

                if(ImGui.BeginMenu(this.MicroBenchmark.ToString()))
                {
                    ImGui.EndMenu();
                }

                ImGui.EndMainMenuBar();
            }
            ImGui.ShowDemoWindow();
        }

        public void Render(RenderTarget2D renderTarget)
        {
            ImGui.Render();
            this.ImGuiRenderer.Render(ImGui.GetDrawData(), renderTarget);
        }

        public void Dispose()
            => this.ImGuiRenderer.Dispose();
    }
}
