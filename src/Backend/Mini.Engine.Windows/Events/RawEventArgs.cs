using System;

namespace Mini.Engine.Windows.Events;

public readonly struct RawEventArgs
{
    public RawEventArgs(IntPtr hWnd, uint msg, UIntPtr wParam, IntPtr lParam)
    {
        this.HWnd = hWnd;
        this.Msg = msg;
        this.WParam = wParam;
        this.LParam = lParam;
    }

    public IntPtr HWnd { get; }
    public uint Msg { get; }
    public UIntPtr WParam { get; }
    public IntPtr LParam { get; }
}
