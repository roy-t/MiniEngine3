using System.Diagnostics;
using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;

namespace Mini.Engine.Windows.Events;

public sealed class WindowEvents
{
    //public EventHandler<SizeEventArgs>? OnResize;
    //public EventHandler<bool>? OnFocus;
    //public EventHandler? OnDestroy;

    private readonly Dictionary<HWND, Win32Window> Windows;

    public WindowEvents()
    {
        this.Windows = new Dictionary<HWND, Win32Window>();
    }

    public void Register(Win32Window window)
    {
        this.Windows.Add(window.Handle, window);
    }

    private void Remove(Win32Window window)
    {
        this.Windows.Remove(window.Handle);
    }

    internal void FireWindowEvents(HWND hWnd, uint msg, UIntPtr wParam, IntPtr lParam)
    {
        if (this.Windows.TryGetValue(hWnd, out var window))
        {
            switch (msg)
            {
                case WM_SIZE:
                    var lp = (int)lParam;
                    var width = Loword(lp);
                    var height = Hiword(lp);

                    switch ((uint)wParam)
                    {
                        case SIZE_RESTORED:
                        case SIZE_MAXIMIZED:
                        case SIZE_MINIMIZED:
                            window.OnSizeChanged(width, height);
                            break;
                    }
                    break;

                case WM_SETFOCUS:
                    window.OnFocusChanged(true);
                    break;

                case WM_KILLFOCUS:
                    window.OnFocusChanged(false);
                    break;

                case WM_ACTIVATE:
                    window.OnFocusChanged(Loword((int)wParam) != 0);
                    break;

                case WM_DESTROY:
                    window.OnDestroyed();
                    this.Windows.Remove(window.Handle);
                    break;
            }
        }
        else
        {
            Debug.WriteLine($"Window not registered: {msg}");
        }
    }

    private static int Loword(int number)
    {
        return number & 0x0000FFFF;
    }

    private static int Hiword(int number)
    {
        return number >> 16;
    }
}
