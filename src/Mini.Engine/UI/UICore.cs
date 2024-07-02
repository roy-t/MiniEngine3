using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.Windows;

using Shader = Mini.Engine.Content.Shaders.Generated.UserInterface;

namespace Mini.Engine.UI;

[Service]
public sealed class UICore : IDisposable
{
    private readonly ImGuiRenderer Renderer;
    //private readonly ImGuiInputHandler Input;
    private readonly ImGuiInputEventListener Input;
    private readonly ImGuiIOPtr IO;
    private readonly Stopwatch Stopwatch;

    public UICore(Win32Window window, Device device, UITextureRegistry textureRegistry, Shader shader)
    {
        ImGui.CreateContext();

        this.IO = ImGui.GetIO();
        this.Renderer = new ImGuiRenderer(device, textureRegistry, shader);
        //this.Input = new ImGuiInputHandler(window.Handle);
        this.Input = new ImGuiInputEventListener(window);
        Win32Application.RegisterInputEventListener(window, this.Input);

        this.Resize(window.Width, window.Height);

        this.Stopwatch = Stopwatch.StartNew();
    }

    public void Resize(int width, int height)
    {
        this.IO.DisplaySize = new Vector2(width, height);
    }

    public void NewFrame()
    {
        var elapsed = (float)this.Stopwatch.Elapsed.TotalSeconds;
        this.Stopwatch.Restart();

        this.IO.DeltaTime = elapsed;
        this.Input.Update();
        ImGui.NewFrame();
    }

    public void Render()
    {
        ImGui.Render();
        this.Renderer.Render(ImGui.GetDrawData());
    }

    public void Dispose()
    {
        this.Renderer.Dispose();
        ImGui.DestroyContext();
    }
}
