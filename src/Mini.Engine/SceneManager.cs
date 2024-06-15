using System.Diagnostics;
using Mini.Engine.Debugging;
using Mini.Engine.DirectX;
using Mini.Engine.Windows;

namespace Mini.Engine;


public sealed class SceneStack : ISceneStack
{
    private readonly Stack<IGameLoop> Scenes;

    public SceneStack()
    {
        this.Scenes = new Stack<IGameLoop>(10);
    }

    public void Push(IGameLoop scene)
    {
        this.Scenes.Push(scene);
    }

    public void ReplaceTop(IGameLoop scene)
    {
        this.Scenes.Pop();
        this.Scenes.Push(scene);
    }

    public void Pop()
    {
        this.Scenes.Pop();
    }
}

public sealed class SceneManager
{
    private readonly Device Device;
    private readonly Win32Window Window;
    private readonly MetricService Metrics;

    public SceneManager(Device device, Win32Window window, MetricService metrics)
    {
        this.Device = device;
        this.Window = window;
        this.Metrics = metrics;
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

            this.Frame((float)alpha, (float)elapsed); // alpha signifies how much to lerp between current and future state

            this.Device.Present();

            if (!this.Window.IsMinimized && (this.Window.Width != this.Device.Width || this.Window.Height != this.Device.Height))
            {
                this.ResizeDeviceResources();
            }

            elapsed = stopwatch.Elapsed.TotalSeconds;
            stopwatch.Restart();
            accumulator += Math.Min(elapsed, 0.1); // cap elapsed on some worst case value to not explode anything

            this.Metrics.Update(nameof(SceneManager) + ".Run.Millis", (float)(elapsed * 1000.0));
            this.Metrics.UpdateBuiltInGauges();
        }
    }

    private void Simulate()
    {
        throw new NotImplementedException();
    }

    private void Frame(float alpha, float elapsed)
    {
        throw new NotImplementedException();
    }

    private void ResizeDeviceResources()
    {
        this.Device.Resize(this.Window.Width, this.Window.Height);
        throw new NotImplementedException(); //this.gameLoop.Resize(this.width, this.height);
    }
}
