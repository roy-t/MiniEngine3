using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using Mini.Engine.Content;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX;
using Mini.Engine.IO;
using Mini.Engine.UI;
using Mini.Engine.Windows;
using Serilog;

using Shader = Mini.Engine.Content.Shaders.Generated.UserInterface;

namespace MultiplayerDemo;

public class Program
{
    [STAThread]
    static void Main(string[] arguments)
    {
        var logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Information()
            .WriteTo.Debug()
            .CreateLogger();


        using var window = Win32Application.Initialize("MultiplayerDemo");
        var input = new SimpleInputService(window);

        using var lifetimeManager = new LifetimeManager(logger);
        var programFrame = lifetimeManager.PushFrame();

        using var device = new Device(window.Handle, window.Width, window.Height, lifetimeManager);

        var fileSystem = new DiskFileSystem(logger, arguments[0]);
        var content = new ContentManager(logger, device, lifetimeManager, fileSystem);
        var shader = new Shader(device, content);

        var uiContext = ImGui.CreateContext();
        var uiIO = ImGui.GetIO();
        uiIO.DisplaySize = new Vector2(window.Width, window.Height);

        var uiTextureRegistry = new UITextureRegistry();
        using var uiRenderer = new ImGuiRenderer(device, uiTextureRegistry, shader);
        var uiInputListener = new ImGuiInputEventListener(window);

        Win32Application.RegisterInputEventListener(window, uiInputListener);

        var stopwatch = new Stopwatch();
        const double dt = 1.0 / 60.0; // main loop tick rate

        // update immediately
        var elapsed = dt;
        var accumulator = dt;

        while (Win32Application.PumpMessages())
        {
            while (accumulator > dt)
            {
                accumulator -= dt;
                // Simulate
            }

            device.ImmediateContext.ClearBackBuffer();

            input.NextFrame();
            uiInputListener.Update();
            ImGui.NewFrame();

            // DO stuff
            ImGui.ShowDemoWindow();


            uiIO.DeltaTime = (float)elapsed;
            ImGui.Render();
            uiRenderer.Render(ImGui.GetDrawData());

            device.Present();

            if (!window.IsMinimized && (window.Width != device.Width || window.Height != device.Height))
            {
                device.Resize(window.Width, window.Height);
                uiIO.DisplaySize = new Vector2(window.Width, window.Height);
            }

            elapsed = stopwatch.Elapsed.TotalSeconds;
            stopwatch.Restart();
            accumulator += Math.Min(elapsed, 0.1); // cap elapsed on some worst case value to not explode anything

            if (input.Keyboard.Pressed(VirtualKeyCode.VK_ESCAPE))
            {
                window.Dispose();
            }
        }

        lifetimeManager.PopFrame(programFrame);
    }
}

