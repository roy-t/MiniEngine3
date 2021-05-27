using System;
using System.Runtime.CompilerServices;
using Mini.Engine.Win32;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using static Mini.Engine.Win32.Kernel32;
using static Mini.Engine.Win32.User32;
using static Vortice.Direct3D11.D3D11;

namespace Mini.Engine
{
    class Program : IDisposable
    {
        private const uint PM_REMOVE = 1;

        [STAThread]
        static void Main()
        {
            using var program = new Program();
            program.Run();
        }

        private readonly ID3D11Device Device;
        private readonly ID3D11DeviceContext DeviceContext;
        private readonly AppWindow window;

        private bool quitRequested;

        private Program()
        {
            D3D11CreateDevice(IntPtr.Zero, DriverType.Hardware, DeviceCreationFlags.None, new[] { FeatureLevel.Level_11_1 },
                out this.Device, out this.DeviceContext);

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
        }

        private void Run()
        {
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

                foreach (var window in windows.Values)
                    window.UpdateAndDraw();
            }
        }

        IntPtr WndProc(IntPtr hWnd, uint msg, UIntPtr wParam, IntPtr lParam)
        {
            AppWindow window;
            windows.TryGetValue(hWnd, out window);

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

        public void Dispose()
        {
            this.DeviceContext.Dispose();
            this.Device.Dispose();
        }
    }
}
