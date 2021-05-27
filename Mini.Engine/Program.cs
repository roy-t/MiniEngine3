using System;
using System.Runtime.CompilerServices;
using Vortice.Win32;
using static Vortice.Win32.Kernel32;
using static Vortice.Win32.User32;

namespace VorticeImGui
{
    public class Program
    {
        private static AppWindow mainWindow;

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

            mainWindow = new AppWindow("Vortice ImGui", 800, 600);
            mainWindow.Show();

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

            mainWindow.Dispose();
        }

        private static IntPtr WndProc(IntPtr hWnd, uint msg, UIntPtr wParam, IntPtr lParam)
        {
            if (mainWindow?.ProcessMessage(msg, wParam, lParam) ?? false)
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
