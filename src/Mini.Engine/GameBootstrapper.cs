using System;
using System.Diagnostics;
using Mini.Engine.Configuration;
using Mini.Engine.Debugging;
using Mini.Engine.DirectX;
using Mini.Engine.Windows;
using Vortice.DXGI;
using VorticeImGui;

namespace Mini.Engine
{
    public sealed class GameBootstrapper : IDisposable
    {
        private readonly Win32Window Window;
        private readonly Device Device;
        private readonly ImGuiPanel Panel;

        public GameBootstrapper(Register registerDelegate)
        {
            RenderDoc.Load(out var renderDoc);

            this.Window = Win32Application.Initialize("Hello World!", 800, 600);
            this.Window.Show();

            this.Device = new Device(this.Window.Handle, Format.R8G8B8A8_UNorm, this.Window.Width, this.Window.Height);
            this.Panel = new ImGuiPanel(renderDoc, this.Device, this.Window.Handle, this.Window.Width, this.Window.Height);

            Win32Application.WindowEvents.OnResize += (o, e) =>
            {
                this.Device.Resize(e.Width, e.Height);
                this.Panel.Resize(e.Width, e.Height);
            };

            registerDelegate(this.Device);
        }

        public void Run()
        {
            var stopWatch = Stopwatch.StartNew();
            while (Win32Application.PumpMessages())
            {
                var elapsed = (float)stopWatch.Elapsed.TotalSeconds;
                stopWatch.Restart();

                this.Device.Clear();
                this.Panel.Render(elapsed, this.Device.BackBuffer);
                this.Device.Present();
            }
        }

        public void Dispose()
        {
            this.Panel.Dispose();
            this.Device.Dispose();
            this.Window.Dispose();
        }
    }
}
