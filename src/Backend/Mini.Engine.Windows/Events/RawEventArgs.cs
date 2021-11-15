using System;
using Vortice.Win32;

namespace Mini.Engine.Windows.Events;

public readonly struct RawEventArgs
{
    public RawEventArgs(IntPtr hWnd, WindowMessage msg, UIntPtr wParam, IntPtr lParam)
    {
        this.HWnd = hWnd;
        this.Msg = msg;
        this.WParam = wParam;
        this.LParam = lParam;
    }

    public IntPtr HWnd { get; }
    public WindowMessage Msg { get; }
    public UIntPtr WParam { get; }
    public IntPtr LParam { get; }
}
