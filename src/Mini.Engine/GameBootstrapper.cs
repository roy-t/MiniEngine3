using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Mini.Engine.Configuration;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.Debugging;
using Mini.Engine.DirectX;
using Mini.Engine.IO;
using Mini.Engine.UI;
using Mini.Engine.Windows;
using Serilog;
using static Windows.Win32.UI.Input.KeyboardAndMouse.VIRTUAL_KEY;

namespace Mini.Engine;

// TODO: this class is getting quite long with a lot of mixed reponsibilities
public sealed class GameBootstrapper
{
    private static readonly ushort Escape = InputService.GetScanCode(VK_ESCAPE);
    private static readonly ushort F1 = InputService.GetScanCode(VK_F1);

    private readonly LifetimeManager LifetimeManager;
    private readonly Win32Window Window;
    private readonly Device Device;
    private readonly DiskFileSystem FileSystem;
    private readonly ILogger Logger;

    private EditorUserInterface ui;
    private IGameLoop gameLoop;

    private readonly InputService InputService;
    private readonly Keyboard Keyboard;
    private bool enableUI = true;

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

        this.enableUI = !StartupArguments.NoUi;

        // Load everything we need to display something
        var gameLoopType = Type.GetType(StartupArguments.GameLoopType, true, true)
            ?? throw new Exception($"Unable to find game loop {StartupArguments.GameLoopType}");

        this.RunLoadingScreenAndLoad(gameLoopType, services);

        this.Logger.Information("Bootstrapping {@gameLoop} took: {@milliseconds}ms", gameLoopType, stopWatch.ElapsedMilliseconds);
    }

    [MemberNotNull(nameof(ui), nameof(gameLoop))]
    private void RunLoadingScreenAndLoad(Type gameLoopType, Services services)
    {
        var loadingScreen = services.Resolve<LoadingScreen>();
        var initializationOrder = InjectableDependencies.CreateInitializationOrder(gameLoopType);
        var serviceActions = initializationOrder.Select(t => new LoadAction(t.Name, () => services.Resolve(t)));

        EditorUserInterface? ui = null;
        IGameLoop? gameLoop = null;

        var actions = new List<LoadAction>(serviceActions)
        {            
            new LoadAction(nameof(EditorUserInterface), () => ui = services.Resolve<EditorUserInterface>()),
            new LoadAction(gameLoopType.Name, () => gameLoop = services.Resolve<IGameLoop>(gameLoopType)),
        };

        loadingScreen.Load(actions, "gameloop");

        this.ui = ui!;

        this.gameLoop = gameLoop!;
    }

    public void Run()
    {
        //return;
        var stopwatch = Stopwatch.StartNew();

        const double dt = 1.0 / 60.0; // constant tick rate of simulation
        var t = 0.0;
        var accumulator = dt; // update immediately
        this.Device.VSync = true;
        while (Win32Application.PumpMessages())
        {
            // Main loop based on https://www.gafferongames.com/post/fix_your_timestep/
            var elapsed = stopwatch.Elapsed.TotalSeconds;
            stopwatch.Restart();

            elapsed = Math.Min(elapsed, 0.25);
            accumulator += elapsed;

#if DEBUG
            foreach(var type in HotReloadManager.GetChangedTypes())
            {
                this.Logger.Warning("{type} was changed", type);
            }
#endif

            if (this.enableUI)
            {
                this.ui.NewFrame((float)elapsed);
            }

            while (accumulator >= dt)
            {
                this.InputService.NextFrame();
                while (this.InputService.ProcessEvents(this.Keyboard))
                {
                    if (this.Keyboard.Pressed(Escape))
                    {
                        this.Window.Dispose();
                    }

                    if (this.Keyboard.Pressed(F1))
                    {
                        this.enableUI = !this.enableUI;
                    }
                }

                // everything that changes on screen should have a current and future state
                // updating it moves both one step forward.
                this.gameLoop.Update((float)t, (float)dt);
                t += dt;
                accumulator -= dt;
            }

            var alpha = accumulator / dt;

            this.Device.ImmediateContext.ClearBackBuffer();
            this.gameLoop.Draw((float)alpha); // alpha signifies how much to lerp between current and future state
            if (this.enableUI)
            {
                this.ui.Render();
            }
            this.Device.Present();

            if (!this.Window.IsMinimized && (this.Window.Width != this.width || this.Window.Height != this.height))
            {
                this.width = this.Window.Width;
                this.height = this.Window.Height;
                this.ResizeDeviceResources();
            }
        }
    }

    private void ResizeDeviceResources()
    {
        this.Device.Resize(this.width, this.height);
        this.ui.Resize(this.width, this.height);
        this.gameLoop.Resize(this.width, this.height);
    }

    private void LoadRenderDoc(Services services)
    {
        if (StartupArguments.EnableRenderDoc)
        {
            var loaded = RenderDoc.Load(out var renderDoc);
            if (loaded)
            {
                services.Register(renderDoc);
                this.Logger.Information("Started RenderDoc");
            }
            else
            {
                this.Logger.Warning("Could not start RenderDoc");
            }
        }
    }
}
