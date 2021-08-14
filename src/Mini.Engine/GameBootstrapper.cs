using System.Diagnostics;
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
        private readonly UserInterface UI;
        private readonly GameLoop GameLoop;

        public GameBootstrapper(ILogger logger,  Register registerDelegate, Resolve resolveDelegate)
        {
            var loaded = RenderDoc.Load(out var renderDoc);
            if(!loaded) { logger.Warning("Could not load RenderDoc"); }

            this.Window = Win32Application.Initialize("Mini.Engine", 1280, 720);
            this.Window.Show();

            this.Device = new Device(this.Window.Handle, Format.R8G8B8A8_UNorm, this.Window.Width, this.Window.Height);
            this.UI = new UserInterface(renderDoc, this.Device, this.Window.Handle, this.Window.Width, this.Window.Height);

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
                var elapsed = (float)stopWatch.Elapsed.TotalSeconds;
                stopWatch.Restart();

                this.Device.ClearBackBuffer();
                this.UI.Update(elapsed);

                this.GameLoop.Draw();

                this.UI.Render();
                this.Device.Present();
            }
        }

        public void Dispose() => this.UI.Dispose();
    }
}
