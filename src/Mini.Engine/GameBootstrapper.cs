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

namespace Mini.Engine
{
    public sealed class GameBootstrapper : IDisposable
    {
        private readonly Win32Window Window;
        private readonly Device Device;
        private readonly IVirtualFileSystem FileSystem;

        private readonly DebugLayerLogger DebugLayerLogger;
        private readonly UserInterface UI;
        private readonly GameLoop GameLoop;
        private readonly ILogger Logger;

        private RenderDoc? renderDoc;

        public GameBootstrapper(ILogger logger, Register register, RegisterAs registerAs, Resolve resolve)
        {
            this.Logger = logger.ForContext<GameBootstrapper>();

            this.Window = Win32Application.Initialize("Mini.Engine", 1280, 720);
            this.Window.Show();

            this.LoadRenderDoc();

            this.Device = new Device(this.Window.Handle, Format.R8G8B8A8_UNorm, this.Window.Width, this.Window.Height, "Device");
            this.FileSystem = new DiskFileSystem(logger, StartupArguments.ContentRoot);

            // Handle ownership/lifetime control over to LightInject
            register(this.Device);
            register(this.Window);
            registerAs(this.FileSystem, typeof(IVirtualFileSystem));

            this.DebugLayerLogger = (DebugLayerLogger)resolve(typeof(DebugLayerLogger));
            this.GameLoop = (GameLoop)resolve(typeof(GameLoop));
            this.UI = (UserInterface)resolve(typeof(UserInterface));
            
            Win32Application.WindowEvents.OnResize += (o, e) =>
            {
                this.Device.Resize(e.Width, e.Height);
                this.UI.Resize(e.Width, e.Height);
            };
        }

        public void Run()
        {
            var stopWatch = Stopwatch.StartNew();
            while (Win32Application.PumpMessages())
            {
                this.DebugLayerLogger.LogMessages();

                var elapsed = (float)stopWatch.Elapsed.TotalSeconds;
                stopWatch.Restart();

                this.Device.ClearBackBuffer();
                this.UI.Update(elapsed);

                this.GameLoop.Update();
                this.GameLoop.Draw();
#if DEBUG
                this.ShowRenderDocUI();
#endif
                this.UI.Render();
                this.Device.Present();
            }
        }

        public void Dispose() => this.UI.Dispose();

        private void ShowRenderDocUI()
        {
            if (this.renderDoc is RenderDoc renderDoc)
            {
                if(ImGui.Begin("RenderDoc"))
                {
                    if(ImGui.MenuItem("Capture"))
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
            if(StartupArguments.EnableRenderDoc)
            {
                var loaded = RenderDoc.Load(out var renderDoc);
                if(loaded)
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
