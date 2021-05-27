using System;
using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using Mini.Engine.Windows;
using Vortice.Win32;

namespace VorticeImGui
{
    internal sealed class AppWindow : DirectXWindow
    {
        private readonly ImGuiRenderer ImGuiRenderer;
        private readonly ImGuiInputHandler ImguiInputHandler;
        private readonly Stopwatch StopWatch = Stopwatch.StartNew();

        private TimeSpan lastFrameTime;

        public AppWindow(string title, int width, int height)
            : base(title, width, height)
        {
            ImGui.CreateContext();
            this.ImGuiRenderer = new ImGuiRenderer(base.Device, base.DeviceContext);
            this.ImguiInputHandler = new ImGuiInputHandler(this.Handle);

            ImGui.GetIO().DisplaySize = new Vector2(this.Width, this.Height);
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

        protected override void Render()
        {
            this.UpdateImGui();
            ImGui.Render();
            this.ImGuiRenderer.Render(ImGui.GetDrawData());
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
            ImGui.ShowDemoWindow();
        }
    }
}
