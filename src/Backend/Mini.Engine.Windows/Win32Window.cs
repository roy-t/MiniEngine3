using System;
using Vortice;
using Vortice.Win32;
using static Vortice.Win32.User32;
using static Vortice.Win32.WindowExStyles;
using static Vortice.Win32.WindowStyles;

namespace Mini.Engine.Windows
{
    public sealed class Win32Window : IDisposable
    {
        internal Win32Window(string title, int width, int height, Events.WindowEvents windowEvents)
        {
            this.Title = title;
            this.Width = width;
            this.Height = height;

            var screenWidth = GetSystemMetrics(SystemMetrics.SM_CXSCREEN);
            var screenHeight = GetSystemMetrics(SystemMetrics.SM_CYSCREEN);
            var x = (screenWidth - this.Width) / 2;
            var y = (screenHeight - this.Height) / 2;

            var style = WS_CAPTION | WS_SYSMENU | WS_MINIMIZEBOX;
            var styleEx = WS_EX_APPWINDOW | WS_EX_WINDOWEDGE;

            var windowRect = new RawRect(0, 0, this.Width, this.Height);
            AdjustWindowRectEx(ref windowRect, style, false, styleEx);

            var windowWidth = windowRect.Right - windowRect.Left;
            var windowHeight = windowRect.Bottom - windowRect.Top;

            var hwnd = CreateWindowEx(
                styleEx, "WndClass", this.Title, (int)style,
                x, y, windowWidth, windowHeight,
                IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

            this.Handle = hwnd;

            windowEvents.OnResize += (o, e) =>
            {
                this.IsMinimized = e.Width == 0 && e.Height == 0;
            };
        }

        public string Title { get; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public IntPtr Handle { get; private set; }
        public bool IsMinimized { get; private set; }

        public void Show()
            => ShowWindow(this.Handle, ShowWindowCommand.Normal);

        public void Dispose()
            => DestroyWindow(this.Handle);
    }
}