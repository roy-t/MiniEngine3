using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;

namespace Mini.Engine.Windows.Events;

public interface IInputEventListener
{
    void OnButtonDown(MouseButton button);
    void OnButtonUp(MouseButton button);
    void OnScroll(float v);
    void OnHScroll(float v);
    void OnChar(char wParam);
    void OnKeyDown(VirtualKeyCode virtualKeyCode);
    void OnKeyUp(VirtualKeyCode virtualKeyCode);
}

public interface IWindowEventListener
{
    bool HasMouseCapture { get; }
    void OnSizeChanged(int width, int height);
    void OnFocusChanged(bool hasFocus);
    void OnDestroyed();
    void OnMouseCapture(bool hasMouseCapture);
}

public sealed class ProcessEvents
{
    private readonly Dictionary<HWND, IWindowEventListener> WindowEventListeners;
    private readonly Dictionary<HWND, IInputEventListener> InputEventListeners;

    public ProcessEvents()
    {
        this.WindowEventListeners = new Dictionary<HWND, IWindowEventListener>();
        this.InputEventListeners = new Dictionary<HWND, IInputEventListener>();
    }

    public void Register(Win32Window window)
    {
        this.WindowEventListeners.Add(window.Handle, window);
    }

    public void Register(HWND windowHandle, IInputEventListener listener)
    {
        this.InputEventListeners.Add(windowHandle, listener);
    }

    internal void FireWindowEvents(HWND hWnd, uint msg, UIntPtr wParam, IntPtr lParam)
    {
        this.WindowEventListeners.TryGetValue(hWnd, out var window);
        this.InputEventListeners.TryGetValue(hWnd, out var listener);

        switch (msg)
        {
            // Window
            case WM_SIZE:
                var lp = lParam.ToInt32();
                var width = EventDecoder.Loword(lp);
                var height = EventDecoder.Hiword(lp);

                switch (wParam.ToUInt32())
                {
                    case SIZE_RESTORED:
                    case SIZE_MAXIMIZED:
                    case SIZE_MINIMIZED:
                        window?.OnSizeChanged(width, height);
                        break;
                }
                break;

            case WM_SETFOCUS:
                window?.OnFocusChanged(true);
                break;

            case WM_KILLFOCUS:
                window?.OnFocusChanged(false);
                break;

            case WM_ACTIVATE:
                window?.OnFocusChanged(EventDecoder.Loword((int)wParam) != 0);
                break;

            case WM_DESTROY:
                if (window != null)
                {
                    window.OnDestroyed();
                    this.WindowEventListeners.Remove(hWnd);
                }
                break;

            // Mouse
            case WM_LBUTTONDOWN:
            case WM_LBUTTONDBLCLK:
            case WM_RBUTTONDOWN:
            case WM_RBUTTONDBLCLK:
            case WM_MBUTTONDOWN:
            case WM_MBUTTONDBLCLK:
            case WM_XBUTTONDOWN:
            case WM_XBUTTONDBLCLK:
                if (window != null && !window.HasMouseCapture)
                {
                    SetCapture(hWnd);
                    window.OnMouseCapture(true);
                }

                listener?.OnButtonDown(EventDecoder.GetMouseButton(msg, wParam, lParam));
                break;

            case WM_LBUTTONUP:
            case WM_RBUTTONUP:
            case WM_MBUTTONUP:
            case WM_XBUTTONUP:
                if (window != null && window.HasMouseCapture)
                {
                    ReleaseCapture();
                    window.OnMouseCapture(false);
                }

                listener?.OnButtonUp(EventDecoder.GetMouseButton(msg, wParam, lParam));
                break;


            case WM_MOUSEWHEEL:
                listener?.OnScroll(EventDecoder.GetMouseWheelDelta(wParam));
                break;

            case WM_MOUSEHWHEEL:
                listener?.OnHScroll(EventDecoder.GetMouseWheelDelta(wParam));
                break;

            // Keyboard
            case WM_CHAR:
                listener?.OnChar((char)wParam);
                break;


            case WM_KEYDOWN:
            case WM_SYSKEYDOWN:
                listener?.OnKeyDown(EventDecoder.GetKeyCode(wParam));
                break;
            case WM_KEYUP:
            case WM_SYSKEYUP:
                listener?.OnKeyUp(EventDecoder.GetKeyCode(wParam));
                break;

            case WM_SETCURSOR:
                // TODO: We ignore WM_SETCURSOR, but that might be useful in the future to detect when a uncaptured mouse
                // first enters the screen so that we can change the cursor icon.
                break;

        }
    }
}

