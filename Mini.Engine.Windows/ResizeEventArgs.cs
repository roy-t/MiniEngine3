using System;

namespace Mini.Engine.Windows
{
    public class ResizeEventArgs : EventArgs
    {
        public ResizeEventArgs(Win32Window window, int width, int height)
        {
            this.Window = window;
            this.Width = width;
            this.Height = height;
        }

        public Win32Window Window { get; }
        public int Width { get; }
        public int Height { get; }
    }
}