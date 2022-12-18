using System.Numerics;
using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.Content.Shaders.Generated;
using Mini.Engine.DirectX;
using Mini.Engine.Windows;

namespace Mini.Engine.UI;

[Service]
public sealed class UICore : IDisposable
{
    private readonly ImGuiRenderer Renderer;
    private readonly ImGuiInputHandler Input;
    private readonly ImGuiIOPtr IO;

    public UICore(Win32Window window, Device device, UITextureRegistry textureRegistry, UserInterface shader)
    {
        ImGui.CreateContext();        

        this.IO = ImGui.GetIO();
        this.Renderer = new ImGuiRenderer(device, textureRegistry, shader);
        this.Input = new ImGuiInputHandler(window.Handle);

        this.Resize(window.Width, window.Height);
    }

    public void Resize(int width, int height)
    {
        this.IO.DisplaySize = new Vector2(width, height);
    }

    public void NewFrame(float elapsed)
    {
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
