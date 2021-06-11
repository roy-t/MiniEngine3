using System;
using Mini.Engine.Debugging;
using Mini.Engine.DirectX;
using Mini.Engine.Windows;
using Vortice.DXGI;
using Vortice.Win32;
using static Vortice.Win32.User32;

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
            using var appWindow = new AppWindow(renderDoc, device, window.Handle, window.Width, window.Height);

            Win32Application.WindowEvents.OnResize += (o, e) =>
            {
                device.Resize(e.Width, e.Height);
                appWindow.Resize(e.Width, e.Height);
            };

            //window.OnMessage += (o, e) => appWindow.ProcessMessage(e.Msg, e.WParam, e.LParam);

            var running = true;
            while (running)
            {
                if (PeekMessage(out var msg, IntPtr.Zero, 0, 0, PM_REMOVE))
                {
                    TranslateMessage(ref msg);
                    DispatchMessage(ref msg);

                    running = msg.Value != (uint)WindowMessage.Quit;
                }

                device.Clear();
                appWindow.Render(device.GetBackBufferView());
                device.Present();
            }

            window.Dispose();
        }
    }
}
