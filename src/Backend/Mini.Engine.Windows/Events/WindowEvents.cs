using System;
using Vortice.Win32;

namespace Mini.Engine.Windows.Events
{
    public sealed class WindowEvents
    {
        public EventHandler<SizeEventArgs>? OnResize;

        internal void FireWindowEvents(IntPtr hWnd, WindowMessage msg, UIntPtr wParam, IntPtr lParam)
        {
            switch (msg)
            {
                case WindowMessage.Size:
                    var lp = (int)lParam;
                    var width = Utils.Loword(lp);
                    var height = Utils.Hiword(lp);


                    switch ((SizeMessage)wParam)
                    {
                        case SizeMessage.SIZE_RESTORED:
                        case SizeMessage.SIZE_MAXIMIZED:
                        case SizeMessage.SIZE_MINIMIZED:
                            this.OnResize?.Invoke(hWnd, new SizeEventArgs(width, height));
                            break;
                    }
                    break;
            }
        }
    }
}
