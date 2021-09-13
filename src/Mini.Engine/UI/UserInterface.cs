using System.Numerics;
using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.DirectX;
using Mini.Engine.Windows;

namespace Mini.Engine.UI
{
    [Service]
    public sealed class UserInterface : IDisposable
    {
        private readonly ImGuiRenderer ImGuiRenderer;
        private readonly ImGuiInputHandler ImguiInputHandler;
        private readonly ImGuiIOPtr IO;
        private readonly MicroBenchmark MicroBenchmark;

        public UserInterface(Win32Window window, Device device, ContentManager content)
        {
            _ = ImGui.CreateContext();
            this.IO = ImGui.GetIO();
            this.ImGuiRenderer = new ImGuiRenderer(device, content);
            this.ImguiInputHandler = new ImGuiInputHandler(window.Handle);
            this.MicroBenchmark = new MicroBenchmark("Perf");

            this.Resize(window.Width, window.Height);
        }

        public void Resize(int width, int height)
            => this.IO.DisplaySize = new Vector2(width, height);

        public void NewFrame(float elapsed)
        {
            this.IO.DeltaTime = elapsed;

            this.ImguiInputHandler.Update();
            this.MicroBenchmark.Update(elapsed);

            ImGui.NewFrame();
            if (ImGui.BeginMainMenuBar())
            {
                ImGui.Text(this.MicroBenchmark.ToString());
                ImGui.EndMainMenuBar();
            }
            ImGui.ShowDemoWindow();
        }

        public void Render()
        {
            ImGui.Render();
            this.ImGuiRenderer.Render(ImGui.GetDrawData());
        }

        public void Dispose()
            => this.ImGuiRenderer.Dispose();
    }
}
