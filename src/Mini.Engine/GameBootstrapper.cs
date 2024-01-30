using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Mini.Engine.Configuration;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.Debugging;
using Mini.Engine.DirectX;
using Mini.Engine.IO;
using Mini.Engine.Windows;
using Serilog;
using static Windows.Win32.UI.Input.KeyboardAndMouse.VIRTUAL_KEY;

namespace Mini.Engine;

// TODO: this class is getting quite long with a lot of mixed reponsibilities
public sealed class GameBootstrapper
{
    private static readonly ushort Escape = InputService.GetScanCode(VK_ESCAPE);

    private readonly LifetimeManager LifetimeManager;
    private readonly Win32Window Window;
    private readonly Device Device;
    private readonly DiskFileSystem FileSystem;
    private readonly ILogger Logger;

    private MetricService metrics;
    private IGameLoop gameLoop;

    private readonly InputService InputService;
    private readonly Keyboard Keyboard;

    private int width;
    private int height;

    public GameBootstrapper(ILogger logger, Services services)
    {
        var stopWatch = Stopwatch.StartNew();

        this.Logger = logger.ForContext<GameBootstrapper>();

        this.Window = Win32Application.Initialize("Mini.Engine");

        this.width = this.Window.Width;
        this.height = this.Window.Height;

        this.LoadRenderDoc(services);

        this.LifetimeManager = new LifetimeManager(this.Logger);
        this.LifetimeManager.PushFrame(nameof(GameBootstrapper));

        this.Device = new Device(this.Window.Handle, this.width, this.height, this.LifetimeManager);
        this.InputService = new InputService(this.Window);
        this.Keyboard = new Keyboard();
        this.FileSystem = new DiskFileSystem(logger, StartupArguments.ContentRoot);

        // Handle ownership/lifetime control over to LightInject
        services.Register(this.LifetimeManager);
        services.Register(this.Device);
        services.Register(this.InputService);
        services.Register(this.Window);
        services.RegisterAs<DiskFileSystem, IVirtualFileSystem>(this.FileSystem);


        // Load everything we need to display something
        var gameLoopType = Type.GetType(StartupArguments.GameLoopType, true, true)
            ?? throw new Exception($"Unable to find game loop {StartupArguments.GameLoopType}");

        this.RunLoadingScreenAndLoad(gameLoopType, services);

        this.Logger.Information("Bootstrapping {@gameLoop} took: {@milliseconds}ms", gameLoopType, stopWatch.ElapsedMilliseconds);
    }

    [MemberNotNull(nameof(gameLoop), nameof(metrics))]
    private void RunLoadingScreenAndLoad(Type gameLoopType, Services services)
    {
        var loadingScreen = services.Resolve<LoadingScreen>();
        var initializationOrder = InjectableDependencies.CreateInitializationOrder(gameLoopType);
        var serviceActions = initializationOrder.Select(t => new LoadAction(t.Name, () => services.Resolve(t)));

        IGameLoop? gameLoop = null;
        MetricService? metrics = null;

        var actions = new List<LoadAction>(serviceActions)
        {
            new LoadAction(gameLoopType.Name, () => gameLoop = services.Resolve<IGameLoop>(gameLoopType)),
            new LoadAction(nameof(MetricService), () => metrics = services.Resolve<MetricService>())
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

        this.LifetimeManager.Clear();
    }

    private void ResizeDeviceResources()
    {
        this.Device.Resize(this.width, this.height);
        this.gameLoop.Resize(this.width, this.height);
    }

    private void LoadRenderDoc(Services services)
    {
        if (StartupArguments.EnableRenderDoc)
        {
            var loaded = RenderDoc.Load(out var renderDoc);
            if (loaded)
            {
                services.Register<RenderDoc?>(renderDoc);
                this.Logger.Information("Started RenderDoc");
            }
            else
            {
                this.Logger.Warning("Could not start RenderDoc");
            }
        }
    }
}
