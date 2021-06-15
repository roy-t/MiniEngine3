using System;
using System.Diagnostics;
using Mini.Engine.Debugging;
using Mini.Engine.DirectX;
using Mini.Engine.Windows;
using Vortice.DXGI;

namespace VorticeImGui
{
    public class Program
    {
        [STAThread]
        static void Main()
        {
            RenderDoc.Load(out var renderDoc);

            var window = Win32Application.Initialize("Hello World!", 800, 600);
            window.Show();

            using var device = new Device(window.Handle, Format.R8G8B8A8_UNorm, window.Width, window.Height);
            using var panel = new ImGuiPanel(renderDoc, device, window.Handle, window.Width, window.Height);

            Win32Application.WindowEvents.OnResize += (o, e) =>
            {
                device.Resize(e.Width, e.Height);
                panel.Resize(e.Width, e.Height);
            };

            var stopWatch = Stopwatch.StartNew();
            while (Win32Application.PumpMessages())
            {
                var elapsed = (float)stopWatch.Elapsed.TotalSeconds;
                stopWatch.Restart();

                device.Clear();
                panel.Render(elapsed, device.BackBuffer);
                device.Present();
            }

            window.Dispose();
        }
    }
}
