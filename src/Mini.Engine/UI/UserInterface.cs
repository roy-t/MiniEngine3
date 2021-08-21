using System.Numerics;
using ImGuiNET;
using Mini.Engine.DirectX;

namespace Mini.Engine.UI
{
    internal sealed class UserInterface : IDisposable
    {
        private readonly ImGuiRenderer ImGuiRenderer;
        private readonly ImGuiInputHandler ImguiInputHandler;
        private readonly ImGuiIOPtr IO;
        private readonly MicroBenchmark MicroBenchmark;


        public UserInterface(Device device, IntPtr windowHandle, int width, int height)
        {
            _ = ImGui.CreateContext();
            this.IO = ImGui.GetIO();
            this.ImGuiRenderer = new ImGuiRenderer(device);
            this.ImguiInputHandler = new ImGuiInputHandler(windowHandle);
            this.MicroBenchmark = new MicroBenchmark("Perf");

            this.Resize(width, height);
        }

        public void Resize(int width, int height)
            => this.IO.DisplaySize = new Vector2(width, height);

        public void Update(float elapsed)
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
