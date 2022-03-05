using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Mini.Engine.Configuration;
using Mini.Engine.Debugging;
using Mini.Engine.DirectX;
using Mini.Engine.IO;
using Mini.Engine.UI;
using Mini.Engine.Windows;
using Serilog;
using Vortice.Win32;

namespace Mini.Engine;

public sealed class GameBootstrapper
{
    private static readonly ushort Escape = InputService.GetScanCode(VK.ESCAPE);
    private static readonly ushort F1 = InputService.GetScanCode(VK.F1);

    private readonly Win32Window Window;
    private readonly Device Device;
    private readonly DiskFileSystem FileSystem;
    private readonly ILogger Logger;

    private DebugLayerLogger debugLayerLogger;
    private EditorUserInterface ui;
    private IGameLoop gameLoop;

    private readonly InputService InputService;
    private readonly Keyboard Keyboard;
    private bool enableUI = true;

    public GameBootstrapper(ILogger logger, Services services)
    {
        var stopWatch = Stopwatch.StartNew();

        this.Logger = logger.ForContext<GameBootstrapper>();

        this.Window = Win32Application.Initialize("Mini.Engine", 1920, 1080);
        this.Window.Show();

        this.LoadRenderDoc(services);

        this.Device = new Device(this.Window.Handle, this.Window.Width, this.Window.Height);
        this.InputService = new InputService(this.Window);
        this.Keyboard = new Keyboard();
        this.FileSystem = new DiskFileSystem(logger, StartupArguments.ContentRoot);

        // Handle ownership/lifetime control over to LightInject
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

    [MemberNotNull(nameof(debugLayerLogger), nameof(ui), nameof(gameLoop))]
    private void RunLoadingScreenAndLoad(Type gameLoopType, Services services)
    {
        var loadingScreen = services.Resolve<LoadingScreen>();        
        var initializationOrder = InjectableDependencies.CreateInitializationOrder(gameLoopType);
        var serviceActions = initializationOrder.Select(t => new LoadAction(t.Name, () => services.Resolve(t)));

        DebugLayerLogger? debugLayerLogger = null;
        EditorUserInterface? ui = null;        
        IGameLoop? gameLoop = null;

        var actions = new List<LoadAction>(serviceActions)
        {
            new LoadAction(nameof(DebugLayerLogger), () => debugLayerLogger = services.Resolve<DebugLayerLogger>()),
            new LoadAction(nameof(EditorUserInterface), () => ui = services.Resolve<EditorUserInterface>()),            
            new LoadAction(gameLoopType.Name, () => gameLoop = services.Resolve<IGameLoop>(gameLoopType)),
        };

        loadingScreen.Load(actions, "gameloop");

        this.debugLayerLogger = debugLayerLogger!;
        this.ui = ui!;
        
        this.gameLoop = gameLoop!;
    }

    public void Run()
    {        
        var stopwatch = Stopwatch.StartNew();

        const double dt = 1.0 / 60.0; // constant tick rate of simulation
        var t = 0.0;
        var accumulator = 0.0;
        this.Device.VSync = true;
        while (Win32Application.PumpMessages())
        {
            // Main loop based on https://www.gafferongames.com/post/fix_your_timestep/
            var elapsed = stopwatch.Elapsed.TotalSeconds;
            stopwatch.Restart();

            elapsed = Math.Min(elapsed, 0.25);
            accumulator += elapsed;

            if (this.enableUI)
            {
                this.ui.NewFrame((float)elapsed);
            }
            this.debugLayerLogger.LogMessages();

            while (accumulator >= dt)
            {
                this.InputService.NextFrame();
                while (this.InputService.ProcessEvents(this.Keyboard))
                {
                    if (this.Keyboard.Pressed(Escape))
                    {
                        this.Window.Dispose();
                    }

                    if(this.Keyboard.Pressed(F1))
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
        }
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
