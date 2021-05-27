using System;
using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using Mini.Engine.Windows;
using Vortice.Win32;

namespace VorticeImGui
{
    class AppWindow : DirectXWindow
    {
        ImGuiRenderer imGuiRenderer;
        ImGuiInputHandler imguiInputHandler;
        Stopwatch stopwatch = Stopwatch.StartNew();
        TimeSpan lastFrameTime;

        IntPtr imGuiContext;

        public AppWindow(string title, int width, int height)
            : base(title, width, height)
        {
            imGuiContext = ImGui.CreateContext();
            ImGui.SetCurrentContext(imGuiContext);

            imGuiRenderer = new ImGuiRenderer(base.Device, base.DeviceContext);
            imguiInputHandler = new ImGuiInputHandler(this.Handle);

            ImGui.GetIO().DisplaySize = new Vector2(this.Width, this.Height);
        }

        public override bool ProcessMessage(uint msg, UIntPtr wParam, IntPtr lParam)
        {
            ImGui.SetCurrentContext(imGuiContext);
            if (imguiInputHandler.ProcessMessage((WindowMessage)msg, wParam, lParam))
                return true;

            return base.ProcessMessage(msg, wParam, lParam);
        }

        protected override void Resize()
        {
            ImGui.GetIO().DisplaySize = new Vector2(this.Width, this.Height);
            base.Resize();
        }

        public virtual void UpdateImGui()
        {
            ImGui.SetCurrentContext(imGuiContext);
            var io = ImGui.GetIO();

            var now = stopwatch.Elapsed;
            var delta = now - lastFrameTime;
            lastFrameTime = now;
            io.DeltaTime = (float)delta.TotalSeconds;

            imguiInputHandler.Update();

            ImGui.NewFrame();
        }

        protected override void Render()
        {
            UpdateImGui();
            ImGui.Render();
            imGuiRenderer.Render(ImGui.GetDrawData());
        }

        public virtual void DoRender() { }
    }
}
