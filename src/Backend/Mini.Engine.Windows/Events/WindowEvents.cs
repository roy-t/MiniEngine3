using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;

namespace Mini.Engine.Windows.Events;

public sealed class WindowEvents
{
    private readonly Dictionary<HWND, Win32Window> Windows;

    public WindowEvents()
    {
        this.Windows = new Dictionary<HWND, Win32Window>();
    }

    public void Register(Win32Window window)
    {
        this.Windows.Add(window.Handle, window);
    }

    internal void FireWindowEvents(HWND hWnd, uint msg, UIntPtr wParam, IntPtr lParam)
    {
        if (this.Windows.TryGetValue(hWnd, out var window))
        {
            switch (msg)
            {
                case WM_SIZE:
                    var lp = lParam.ToInt32();
                    var width = Loword(lp);
                    var height = Hiword(lp);

                    switch (wParam.ToUInt32())
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

                case WM_LBUTTONDOWN:
                case WM_LBUTTONDBLCLK:
                case WM_RBUTTONDOWN:
                case WM_RBUTTONDBLCLK:
                case WM_MBUTTONDOWN:
                case WM_MBUTTONDBLCLK:
                case WM_XBUTTONDOWN:
                case WM_XBUTTONDBLCLK:
                    if (!window.HasMouseCapture)
                    {
                        SetCapture(hWnd);
                        window.OnMouseCapture(true);
                    }
                    break;

                case WM_LBUTTONUP:
                case WM_RBUTTONUP:
                case WM_MBUTTONUP:
                case WM_XBUTTONUP:
                    if (window.HasMouseCapture)
                    {
                        ReleaseCapture();
                        window.OnMouseCapture(false);
                    }
                    break;
            }
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

