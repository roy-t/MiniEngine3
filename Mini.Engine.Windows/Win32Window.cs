using System;
using Vortice;
using Vortice.Win32;
using static Vortice.Win32.User32;

namespace Mini.Engine.Windows
{
    public abstract class Win32Window : IDisposable
    {
        public Win32Window(string title, int width, int height)
        {
            this.Title = title;
            this.Width = width;
            this.Height = height;

            var screenWidth = GetSystemMetrics(SystemMetrics.SM_CXSCREEN);
            var screenHeight = GetSystemMetrics(SystemMetrics.SM_CYSCREEN);
            var x = (screenWidth - this.Width) / 2;
            var y = (screenHeight - this.Height) / 2;

            var style = WindowStyles.WS_OVERLAPPEDWINDOW;
            var styleEx = WindowExStyles.WS_EX_APPWINDOW | WindowExStyles.WS_EX_WINDOWEDGE;

            var windowRect = new RawRect(0, 0, this.Width, this.Height);
            AdjustWindowRectEx(ref windowRect, style, false, styleEx);

            var windowWidth = windowRect.Right - windowRect.Left;
            var windowHeight = windowRect.Bottom - windowRect.Top;

            var hwnd = CreateWindowEx(
                (int)styleEx, "WndClass", this.Title, (int)style,
                x, y, windowWidth, windowHeight,
                IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

            this.Handle = hwnd;
        }

        public string Title { get; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public IntPtr Handle { get; private set; }
        public bool IsMinimized { get; private set; }

        public void Show()
            => ShowWindow(this.Handle, ShowWindowCommand.Normal);

        public virtual bool ProcessMessage(uint msg, UIntPtr wParam, IntPtr lParam)
        {
            switch ((WindowMessage)msg)
            {
                case WindowMessage.Size:
                    switch ((SizeMessage)wParam)
                    {
                        case SizeMessage.SIZE_RESTORED:
                        case SizeMessage.SIZE_MAXIMIZED:
                            this.IsMinimized = false;

                            var lp = (int)lParam;
                            this.Width = Utils.Loword(lp);
                            this.Height = Utils.Hiword(lp);

                            this.Resize();
                            break;
                        case SizeMessage.SIZE_MINIMIZED:
                            this.IsMinimized = true;
                            break;
                        default:
                            break;
                    }
                    break;
            }

            return false;
        }

        protected abstract void Resize();

        public virtual void Dispose()
        {
            if (this.Handle != IntPtr.Zero)
            {
                DestroyWindow(this.Handle);
                this.Handle = IntPtr.Zero;
            }
        }
    }
}