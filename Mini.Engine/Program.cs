using System;
using System.Runtime.CompilerServices;
using Mini.Engine.Debugging;
using Mini.Engine.DirectX;
using Mini.Engine.Windows;
using Vortice.DXGI;
using Vortice.Win32;
using static Vortice.Win32.Kernel32;
using static Vortice.Win32.User32;

namespace VorticeImGui
{
    public class Program
    {
        private static Win32Window window;


        [STAThread]
        static void Main()
        {
            var quitRequested = false;
            var moduleHandle = GetModuleHandle(null);

            var wndClass = new WNDCLASSEX
            {
                Size = Unsafe.SizeOf<WNDCLASSEX>(),
                Styles = WindowClassStyles.CS_HREDRAW | WindowClassStyles.CS_VREDRAW | WindowClassStyles.CS_OWNDC,
                WindowProc = WndProc,
                InstanceHandle = moduleHandle,
                CursorHandle = LoadCursor(IntPtr.Zero, SystemCursor.IDC_ARROW),
                BackgroundBrushHandle = IntPtr.Zero,
                IconHandle = IntPtr.Zero,
                ClassName = "WndClass",
            };

            RegisterClassEx(ref wndClass);

            RenderDoc.Load(out var renderDoc);

            window = new Win32Window("Hell World!", 800, 600);
            window.Show();

            using var device = new Device(window.Handle, Format.R8G8B8A8_UNorm, window.Width, window.Height);
            window.OnResize += (o, e) => device.Resize(e.Width, e.Height);

            while (!quitRequested)
            {
                if (PeekMessage(out var msg, IntPtr.Zero, 0, 0, PM_REMOVE))
                {
                    TranslateMessage(ref msg);
                    DispatchMessage(ref msg);

                    if (msg.Value == (uint)WindowMessage.Quit)
                    {
                        quitRequested = true;
                        break;
                    }
                }

                mainWindow.Frame();
            }

            window.Dispose();
        }

        private static IntPtr WndProc(IntPtr hWnd, uint msg, UIntPtr wParam, IntPtr lParam)
        {
            if (window?.ProcessMessage(msg, wParam, lParam) ?? false)
                return IntPtr.Zero;

            switch ((WindowMessage)msg)
            {
                case WindowMessage.Destroy:
                    PostQuitMessage(0);
                    break;
            }

            return DefWindowProc(hWnd, msg, wParam, lParam);
        }
    }
}
