using System;
using System.Diagnostics;
using Mini.Engine.Configuration;
using Mini.Engine.Debugging;
using Mini.Engine.DirectX;
using Mini.Engine.IO;
using Mini.Engine.UI;
using Mini.Engine.Windows;
using Serilog;
using Vortice.Win32;

namespace Mini.Engine;

public sealed class GameBootstrapper : IDisposable
{
    private static readonly ushort Escape = InputService.GetScanCode(VK.ESCAPE);
    private static readonly ushort F1 = InputService.GetScanCode(VK.F1);

    private readonly Win32Window Window;
    private readonly Device Device;
    private readonly DiskFileSystem FileSystem;

    private readonly DebugLayerLogger DebugLayerLogger;
    private readonly UserInterface UI;
    private readonly IGameLoop GameLoop;
    private readonly ILogger Logger;

    private readonly InputService InputService;
    private readonly Keyboard Keyboard;

    private bool enableUI = true;

    public GameBootstrapper(ILogger logger, Services services)
    {
        var stopWatch = Stopwatch.StartNew();
        Load();


        this.Logger = logger.ForContext<GameBootstrapper>();

        this.Window = Win32Application.Initialize("Mini.Engine", 1920, 1080);
        this.Window.Show();

        this.LoadRenderDoc(services);

        this.Device = new Device(this.Window.Handle, this.Window.Width, this.Window.Height);
        this.FileSystem = new DiskFileSystem(logger, StartupArguments.ContentRoot);

        this.InputService = new InputService(this.Window);
        this.Keyboard = new Keyboard();

        // Handle ownership/lifetime control over to LightInject
        services.Register(this.Device);
        services.Register(this.InputService);
        services.Register(this.Window);
        services.RegisterAs<DiskFileSystem, IVirtualFileSystem>(this.FileSystem);

        this.DebugLayerLogger = services.Resolve<DebugLayerLogger>();
        this.UI = services.Resolve<UserInterface>();
        this.enableUI = !StartupArguments.NoUi;

        var gameLoopType = StartupArguments.GameLoopType;
        this.GameLoop = services.Resolve<IGameLoop>(Type.GetType(gameLoopType, true, true)!);

        Debug.WriteLine($"Loading took: {stopWatch.ElapsedMilliseconds}ms");
    }

    private void Load()
    {
        var type = Type.GetType(StartupArguments.GameLoopType, true, true)
            ?? throw new Exception($"Unable to find game loop {StartupArguments.GameLoopType}");

        var graph = DependencyGraph.Build(type);

        graph.ToString();
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
                this.UI.NewFrame((float)elapsed);
            }
            this.DebugLayerLogger.LogMessages();

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
                this.GameLoop.Update((float)t, (float)dt);
                t += dt;
                accumulator -= dt;
            }

            var alpha = accumulator / dt;
            this.Device.ClearBackBuffer();
            this.GameLoop.Draw((float)alpha); // alpha signifies how much to lerp between current and future state
            if (this.enableUI)
            {
                this.UI.Render();
            }
            this.Device.Present();
        }
    }

    public void Dispose()
    {
        this.UI.Dispose();
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
