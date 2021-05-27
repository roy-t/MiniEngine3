using System;
using System.Runtime.CompilerServices;
using ImGuiNET;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.Win32;
using static Vortice.Win32.Kernel32;
using static Vortice.Win32.User32;

namespace VorticeImGui
{
    class MainWindow : AppWindow
    {
        public MainWindow(Win32Window win32window, ID3D11Device device, ID3D11DeviceContext deviceContext) : base(win32window, device, deviceContext)
        {
        }

        public override void UpdateImGui()
        {
            base.UpdateImGui();
            ImGui.ShowDemoWindow();
        }
    }

    class Program
    {
        [STAThread]
        static void Main()
        {
            new Program().Run();
        }

        bool quitRequested;

        ID3D11Device device;
        ID3D11DeviceContext deviceContext;
        MainWindow mainWindow;

        void Run()
        {
            D3D11.D3D11CreateDevice(null, DriverType.Hardware, DeviceCreationFlags.None, null, out device, out deviceContext);

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

            var win32window = new Win32Window("Vortice ImGui", 800, 600);
            mainWindow = new MainWindow(win32window, device, deviceContext);
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

                mainWindow.UpdateAndDraw();
            }
        }

        private IntPtr WndProc(IntPtr hWnd, uint msg, UIntPtr wParam, IntPtr lParam)
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
