using System;
using Vortice.Win32;
using static Vortice.Win32.SizeMessage;
using static Vortice.Win32.WindowMessage;

namespace Mini.Engine.Windows.Events
{
    public sealed class WindowEvents
    {
        public EventHandler<SizeEventArgs>? OnResize;

        internal void FireWindowEvents(IntPtr hWnd, WindowMessage msg, UIntPtr wParam, IntPtr lParam)
        {
            switch (msg)
            {
                case Size:
                    var lp = (int)lParam;
                    var width = Utils.Loword(lp);
                    var height = Utils.Hiword(lp);

                    switch ((SizeMessage)wParam)
                    {
                        case SIZE_RESTORED:
                        case SIZE_MAXIMIZED:
                        case SIZE_MINIMIZED:
                            this.OnResize?.Invoke(hWnd, new SizeEventArgs(width, height));
                            break;
                    }
                    break;
            }
        }
    }
}
