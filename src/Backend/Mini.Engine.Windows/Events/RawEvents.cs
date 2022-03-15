using System;

namespace Mini.Engine.Windows.Events;

public sealed class RawEvents
{
    public event EventHandler<RawEventArgs>? OnEvent;

    internal void FireWindowEvents(IntPtr hWnd, uint msg, UIntPtr wParam, IntPtr lParam)
    {
        this.OnEvent?.Invoke(hWnd, new RawEventArgs(hWnd, msg, wParam, lParam));
    }
}
