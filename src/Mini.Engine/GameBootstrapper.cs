using System.Diagnostics;
using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.Debugging;
using Mini.Engine.DirectX;
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
        private readonly DebugLayerLogger DebugLayerLogger;
        private readonly UserInterface UI;
        private readonly GameLoop GameLoop;
        private readonly ILogger Logger;

        private RenderDoc? renderDoc;

        public GameBootstrapper(ILogger logger,  Register registerDelegate, Resolve resolveDelegate)
        {
            this.Logger = logger.ForContext<GameBootstrapper>();

            this.Window = Win32Application.Initialize("Mini.Engine", 1280, 720);
            this.Window.Show();

            this.LoadRenderDoc();

            this.Device = new Device(this.Window.Handle, Format.R8G8B8A8_UNorm, this.Window.Width, this.Window.Height, "Device");
            this.DebugLayerLogger = new DebugLayerLogger(this.Device, logger);
            this.UI = new UserInterface(this.Device, this.Window.Handle, this.Window.Width, this.Window.Height);

            Win32Application.WindowEvents.OnResize += (o, e) =>
            {
                this.Device.Resize(e.Width, e.Height);
                this.UI.Resize(e.Width, e.Height);
            };

            // Handle ownership/lifetime control over to LightInject
            registerDelegate(this.Device);
            registerDelegate(this.Window);

            // Lifetime already handled by LightInject
            this.GameLoop = (GameLoop)resolveDelegate(typeof(GameLoop));
            
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

                this.GameLoop.Draw();

                this.ShowRenderDocUI();

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
