using System;
using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using Mini.Engine.Debugging;
using Mini.Engine.DirectX;
using Vortice.Direct3D11;

namespace VorticeImGui
{
    // TODO: clean up
    internal sealed class ImGuiPanel : IDisposable
    {
        private readonly ImGuiRenderer ImGuiRenderer;
        private readonly ImGuiInputHandler ImguiInputHandler;
        private readonly Stopwatch StopWatch = Stopwatch.StartNew();
        private readonly RenderDoc RenderDoc;
        private readonly bool EnableRenderDoc;

        private TimeSpan lastFrameTime;

        public ImGuiPanel(RenderDoc renderDoc, Device device, IntPtr windowHandle, int width, int height)
        {
            ImGui.CreateContext();
            this.ImGuiRenderer = new ImGuiRenderer(device.GetDevice(), device.GetImmediateContext());
            this.ImguiInputHandler = new ImGuiInputHandler(windowHandle);

            ImGui.GetIO().DisplaySize = new Vector2(width, height);

            if (renderDoc != null)
            {
                this.EnableRenderDoc = true;
                this.RenderDoc = renderDoc;
                renderDoc.OverlayEnabled = false;
            }

        }

        public void Resize(int width, int height)
        {
            ImGui.GetIO().DisplaySize = new Vector2(width, height);
        }

        public void Render(ID3D11RenderTargetView renderView)
        {
            this.UpdateImGui();
            ImGui.Render();
            this.ImGuiRenderer.Render(ImGui.GetDrawData(), renderView);
        }

        private void UpdateImGui()
        {
            var io = ImGui.GetIO();

            var now = this.StopWatch.Elapsed;
            var delta = now - this.lastFrameTime;
            this.lastFrameTime = now;
            io.DeltaTime = (float)delta.TotalSeconds;

            this.ImguiInputHandler.Update();

            ImGui.NewFrame();
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("RenderDoc", this.EnableRenderDoc))
                {

                    if (ImGui.MenuItem("Launch Replay UI"))
                    {
                        this.RenderDoc.LaunchReplayUI();
                    }

                    if (ImGui.MenuItem("Capture"))
                    {
                        this.RenderDoc.TriggerCapture();
                    }

                    if (ImGui.MenuItem("Open Last Capture", this.RenderDoc.GetNumCaptures() > 0))
                    {
                        var path = this.RenderDoc.GetCapture(this.RenderDoc.GetNumCaptures() - 1);
                        this.RenderDoc.LaunchReplayUI(path);
                    }

                    ImGui.EndMenu();
                }

                ImGui.EndMainMenuBar();
            }
            ImGui.ShowDemoWindow();
        }

        public void Dispose()
        {
            // TODO:
        }
    }
}
