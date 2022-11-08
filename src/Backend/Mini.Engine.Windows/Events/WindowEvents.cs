using static Windows.Win32.PInvoke;

namespace Mini.Engine.Windows.Events;

public sealed class WindowEvents
{
    public EventHandler<SizeEventArgs>? OnResize;
    public EventHandler<bool>? OnFocus;
    public EventHandler? OnDestroy;

    internal void FireWindowEvents(global::Windows.Win32.Foundation.HWND hWnd, uint msg, UIntPtr wParam, IntPtr lParam)
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
                        this.OnResize?.Invoke(hWnd, new SizeEventArgs(width, height));
                        break;
                }
                break;

            case WM_SETFOCUS:
                this.OnFocus?.Invoke(hWnd, true);
                break;

            case WM_KILLFOCUS:
                this.OnFocus?.Invoke(hWnd, false);
                break;

            case WM_ACTIVATE:
                this.OnFocus?.Invoke(hWnd, Loword((int)wParam) != 0);
                break;

            case WM_DESTROY:
                this.OnDestroy?.Invoke(hWnd, EventArgs.Empty);
                break;
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
