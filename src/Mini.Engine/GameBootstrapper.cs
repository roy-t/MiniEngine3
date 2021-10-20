using System;
using System.Diagnostics;
using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.Debugging;
using Mini.Engine.DirectX;
using Mini.Engine.IO;
using Mini.Engine.UI;
using Mini.Engine.Windows;
using Serilog;
using Vortice.DXGI;
using Vortice.Win32;

namespace Mini.Engine
{
    public sealed class GameBootstrapper : IDisposable
    {
        private static readonly ushort Escape = InputService.GetScanCode(VK.ESCAPE);

        private readonly Win32Window Window;
        private readonly Device Device;
        private readonly IVirtualFileSystem FileSystem;

        private readonly DebugLayerLogger DebugLayerLogger;
        private readonly UserInterface UI;
        private readonly GameLoop GameLoop;
        private readonly ILogger Logger;

        private RenderDoc? renderDoc;

        private readonly InputService InputService;
        private readonly Keyboard Keyboard;

        public GameBootstrapper(ILogger logger, Register register, RegisterAs registerAs, Resolve resolve)
        {
            this.Logger = logger.ForContext<GameBootstrapper>();

            this.Window = Win32Application.Initialize("Mini.Engine", 1280, 720);
            this.Window.Show();

            this.LoadRenderDoc();

            this.Device = new Device(this.Window.Handle, Format.R8G8B8A8_UNorm, this.Window.Width, this.Window.Height, "Device");
            this.FileSystem = new DiskFileSystem(logger, StartupArguments.ContentRoot);

            this.InputService = new InputService(this.Window);
            this.Keyboard = new Keyboard();

            // Handle ownership/lifetime control over to LightInject
            register(this.Device);
            register(this.InputService);
            register(this.Window);
            registerAs(this.FileSystem, typeof(IVirtualFileSystem));

            this.DebugLayerLogger = (DebugLayerLogger)resolve(typeof(DebugLayerLogger));
            this.GameLoop = (GameLoop)resolve(typeof(GameLoop));
            this.UI = (UserInterface)resolve(typeof(UserInterface));
        }

        public void Run()
        {
            var stopwatch = Stopwatch.StartNew();

            const double dt = 1.0 / 60.0; // constant tick rate of simulation
            var t = 0.0;
            var accumulator = 0.0;

            while (Win32Application.PumpMessages())
            {
                // Main loop based on https://www.gafferongames.com/post/fix_your_timestep/
                var elapsed = stopwatch.Elapsed.TotalSeconds;
                stopwatch.Restart();

                elapsed = Math.Min(elapsed, 0.25);
                accumulator += elapsed;

                this.UI.NewFrame((float)elapsed);
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
                    }

                    // everything that changes on screen should have a current and future state
                    // updating it moves both one step forward.
                    this.GameLoop.Update((float)t, (float)dt);
                    t += dt;
                    accumulator -= dt;
                }

                var alpha = accumulator / dt;
                this.Device.ClearBackBuffer();
                this.GameLoop.Draw((float)alpha); // alpha signifies how much to lerp betwen current and future state
#if DEBUG
                this.ShowRenderDocUI();
#endif
                this.UI.Render();
                this.Device.Present();

            }
        }

        public void Dispose()
        {
            this.UI.Dispose();
        }

        private void ShowRenderDocUI()
        {
            if (this.renderDoc is RenderDoc renderDoc)
            {
                if (ImGui.Begin("RenderDoc"))
                {
                    if (ImGui.MenuItem("Capture"))
                    {
                        renderDoc.TriggerCapture();
                    }

                    if (ImGui.MenuItem("Open Last Capture", renderDoc.GetNumCaptures() > 0))
                    {
                        var path = renderDoc.GetCapture(renderDoc.GetNumCaptures() - 1);
                        renderDoc.LaunchReplayUI(path);
                    }
                    ImGui.End();
                }
            }
        }

        private void LoadRenderDoc()
        {
            if (StartupArguments.EnableRenderDoc)
            {
                var loaded = RenderDoc.Load(out var renderDoc);
                if (loaded)
                {
                    this.renderDoc = renderDoc;
                    this.Logger.Information("Started RenderDoc");
                }
                else
                {
                    this.Logger.Warning("Could not start RenderDoc");
                }
            }
        }
    }
}
