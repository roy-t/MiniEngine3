using System;

namespace Mini.Engine.Windows
{
    public sealed class WindowMessageEventArgs : EventArgs
    {
        public WindowMessageEventArgs(Win32Window window, uint msg, UIntPtr wParam, IntPtr lParam)
        {
            this.Window = window;
            this.Msg = msg;
            this.WParam = wParam;
            this.LParam = lParam;
        }

        public Win32Window Window { get; }
        public uint Msg { get; }
        public UIntPtr WParam { get; }
        public IntPtr LParam { get; }
    }
}