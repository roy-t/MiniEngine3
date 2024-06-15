using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using LightInject;
using Mini.Engine.Configuration;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.Debugging;
using Mini.Engine.DirectX;
using Mini.Engine.Windows;
using static Windows.Win32.UI.Input.KeyboardAndMouse.VIRTUAL_KEY;

namespace Mini.Engine;

// TODO: this class is getting quite long with a lot of mixed reponsibilities
public sealed class GameBootstrapper
{
    private static readonly ushort Escape = InputService.GetScanCode(VK_ESCAPE);

    private readonly LifetimeManager LifetimeManager;
    private readonly Win32Window Window;
    private readonly Device Device;

    private MetricService metrics;
    private IGameLoop gameLoop;

    private readonly InputService InputService;
    private readonly Keyboard Keyboard;

    private int width;
    private int height;

    public GameBootstrapper(Win32Window window, InputService inputService, LifetimeManager lifetimeManager, Device device, IServiceFactory factory)
    {
        this.Window = window;
        this.width = window.Width;
        this.height = window.Height;

        this.InputService = inputService;

        this.LifetimeManager = lifetimeManager;
        this.LifetimeManager.PushFrame(nameof(GameBootstrapper));

        this.Device = device;

        this.Keyboard = new Keyboard();

        // Load everything we need to display something
        var gameLoopType = Type.GetType(StartupArguments.GameLoopType, true, true)
            ?? throw new Exception($"Unable to find game loop {StartupArguments.GameLoopType}");

        this.RunLoadingScreenAndLoad(gameLoopType, factory);
    }

    [MemberNotNull(nameof(gameLoop), nameof(metrics))]
    private void RunLoadingScreenAndLoad(Type gameLoopType, IServiceFactory factory)
    {
        var loadingScreen = factory.GetInstance<LoadingScreen>();
        var initializationOrder = InjectableDependencies.CreateInitializationOrder(gameLoopType);
        var serviceActions = initializationOrder.Select(t => new LoadAction(t.Name, () => factory.GetInstance(t)));

        IGameLoop? gameLoop = null;
        MetricService? metrics = null;

        var actions = new List<LoadAction>(serviceActions)
        {
            new LoadAction(gameLoopType.Name, () => gameLoop = (IGameLoop)factory.GetInstance(gameLoopType)),
            new LoadAction(nameof(MetricService), () => metrics = factory.GetInstance<MetricService>())
        };

        loadingScreen.Load(actions, "gameloop");

        this.gameLoop = gameLoop!;
        this.metrics = metrics!;
    }

    public void Run()
    {

        var stopwatch = new Stopwatch();
        const double dt = 1.0 / 60.0; // constant tick rate of simulation

        // update immediately
        var elapsed = dt;
        var accumulator = dt;

        this.Device.VSync = true;
        // Main loop based on https://www.gafferongames.com/post/fix_your_timestep/            
        while (Win32Application.PumpMessages())
        {
            while (accumulator >= dt)
            {
                accumulator -= dt;
                this.gameLoop.Simulation();
            }

            // Note: input should be handled in the draw method, not the simulation method
            this.InputService.NextFrame();
            while (this.InputService.ProcessEvents(this.Keyboard))
            {
                if (this.Keyboard.Pressed(Escape))
                {
                    this.Window.Dispose();
                }
            }

            var alpha = accumulator / dt;

            this.Device.ImmediateContext.ClearBackBuffer();
            this.gameLoop.Frame((float)alpha, (float)elapsed); // alpha signifies how much to lerp between current and future state

            this.Device.Present();

            if (!this.Window.IsMinimized && (this.Window.Width != this.width || this.Window.Height != this.height))
            {
                this.width = this.Window.Width;
                this.height = this.Window.Height;
                this.ResizeDeviceResources();
            }

            elapsed = stopwatch.Elapsed.TotalSeconds;
            stopwatch.Restart();
            accumulator += Math.Min(elapsed, 0.1); // cap elapsed on some worst case value to not explode anything

            this.metrics.Update("GameBootstrapper.Run.Millis", (float)(elapsed * 1000.0));
            this.metrics.UpdateBuiltInGauges();
        }

        this.LifetimeManager.PopFrame(nameof(GameBootstrapper));
    }

    private void ResizeDeviceResources()
    {
        this.Device.Resize(this.width, this.height);
        this.gameLoop.Resize(this.width, this.height);
    }
}
