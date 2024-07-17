using System.Diagnostics;
using Mini.Engine.Configuration;
using Mini.Engine.Debugging;
using Mini.Engine.DirectX;
using Mini.Engine.UI;
using Mini.Engine.Windows;

namespace Mini.Engine;

[Service]
public sealed class GameManager
{
    private readonly Device Device;
    private readonly Win32Window Window;
    private readonly UICore UserInterfaceCore;
    private readonly SimpleInputService Input;
    private readonly MetricService Metrics;
    private readonly SceneStack Scenes;

    public GameManager(Device device, Win32Window window, UICore uiCore, SimpleInputService input, MetricService metrics, SceneStack sceneStack)
    {
        this.Device = device;
        this.Window = window;
        this.UserInterfaceCore = uiCore;
        this.Input = input;
        this.Metrics = metrics;
        this.Scenes = sceneStack;
    }

    public void Run()
    {
        var stopwatch = new Stopwatch();
        const double dt = 1.0 / 60.0; // constant tick rate of simulation

        // update immediately
        var elapsed = dt;
        var accumulator = dt;

        // Main loop based on https://www.gafferongames.com/post/fix_your_timestep/            
        while (Win32Application.PumpMessages())
        {
            while (accumulator >= dt)
            {
                accumulator -= dt;
                this.Simulate();
            }

            var alpha = accumulator / dt;

            this.Device.ImmediateContext.ClearBackBuffer();

            this.Input.NextFrame();

            this.HandleInput((float)elapsed);

            this.Frame((float)alpha, (float)elapsed); // alpha signifies how much to lerp between current and future state

            this.Device.Present();

            if (!this.Window.IsMinimized && (this.Window.Width != this.Device.Width || this.Window.Height != this.Device.Height))
            {
                this.ResizeDeviceResources();
            }

            elapsed = stopwatch.Elapsed.TotalSeconds;
            stopwatch.Restart();
            accumulator += Math.Min(elapsed, 0.1); // cap elapsed on some worst case value to not explode anything

            this.Metrics.Update(nameof(GameManager) + ".Run.Millis", (float)(elapsed * 1000.0));
            this.Metrics.UpdateBuiltInGauges();

#if DEBUG   // Instant quit on ESC
            if (this.Input.Keyboard.Pressed(VirtualKeyCode.VK_ESCAPE))
            {
                this.Window.Dispose();
            }
#endif
        }

        this.Scenes.Clear();
    }

    // Simulate any active scene as they might only partially overlap
    private void Simulate()
    {
        this.Scenes.ForEach(s => s.Simulate());
    }

    // Only the top scene gets to handle input
    private void HandleInput(float elapsedRealWorldTime)
    {
        this.Scenes.Peek().HandleInput(elapsedRealWorldTime);
    }

    // Render any active scene as they might only partially overlap
    private void Frame(float alpha, float elapsed)
    {
        this.UserInterfaceCore.NewFrame(elapsed);
        this.Scenes.ForEach(s => s.Frame(alpha, elapsed));

        this.UserInterfaceCore.Render();
    }

    private void ResizeDeviceResources()
    {
        this.Device.Resize(this.Window.Width, this.Window.Height);
        this.UserInterfaceCore.Resize(this.Window.Width, this.Window.Height);

        this.Scenes.ForEach(s => s.Resize(this.Window.Width, this.Window.Height));
    }
}
