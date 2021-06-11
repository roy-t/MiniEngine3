using System;
using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using Mini.Engine.Debugging;
using Vortice.Direct3D11;
using Vortice.Win32;

namespace VorticeImGui
{
    internal sealed class AppWindow
    {
        private readonly ImGuiRenderer ImGuiRenderer;
        private readonly ImGuiInputHandler ImguiInputHandler;
        private readonly Stopwatch StopWatch = Stopwatch.StartNew();
        private readonly RenderDoc RenderDoc;
        private readonly bool EnableRenderDoc;

        private TimeSpan lastFrameTime;

        public AppWindow(string title, RenderDoc renderDoc, int width, int height)
            : base(title, width, height)
        {
            ImGui.CreateContext();
            this.ImGuiRenderer = new ImGuiRenderer(base.Device, base.DeviceContext);
            this.ImguiInputHandler = new ImGuiInputHandler(this.Handle);

            ImGui.GetIO().DisplaySize = new Vector2(this.Width, this.Height);

            if (renderDoc != null)
            {
                this.EnableRenderDoc = true;
                this.RenderDoc = renderDoc;
                renderDoc.OverlayEnabled = false;
            }

        }

        public override bool ProcessMessage(uint msg, UIntPtr wParam, IntPtr lParam)
        {
            if (this.ImguiInputHandler.ProcessMessage((WindowMessage)msg, wParam, lParam))
            {
                return true;
            }

            return base.ProcessMessage(msg, wParam, lParam);
        }

        protected override void Resize()
        {
            ImGui.GetIO().DisplaySize = new Vector2(this.Width, this.Height);
            base.Resize();
        }

        protected override void Render(ID3D11RenderTargetView renderView)
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
    }
}
