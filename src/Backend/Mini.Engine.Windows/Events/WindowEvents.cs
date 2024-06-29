using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;

namespace Mini.Engine.Windows.Events;

public interface IInputEventListener
{
    void OnButtonDown(MouseButtons button);
    void OnButtonUp(MouseButtons button);
    void OnScroll(float v);
    void OnHScroll(float v);
    void OnChar(char wParam);
}

public sealed class WindowEvents
{
    private readonly Dictionary<HWND, Win32Window> Windows;
    private readonly Dictionary<HWND, IInputEventListener> InputEventListeners;

    public WindowEvents()
    {
        this.Windows = new Dictionary<HWND, Win32Window>();
        this.InputEventListeners = new Dictionary<HWND, IInputEventListener>();
    }

    public void Register(Win32Window window)
    {
        this.Windows.Add(window.Handle, window);
    }

    public void Register(Win32Window window, IInputEventListener listener)
    {
        this.InputEventListeners.Add(window.Handle, listener);
    }

    internal void FireWindowEvents(HWND hWnd, uint msg, UIntPtr wParam, IntPtr lParam)
    {
        this.Windows.TryGetValue(hWnd, out var window);
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
                    this.Windows.Remove(window.Handle);
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

                if (listener != null)
                {
                    var button = EventDecoder.GetMouseButton(msg, wParam, lParam);
                    listener.OnButtonDown(button);
                }
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

                if (listener != null)
                {
                    var button = EventDecoder.GetMouseButton(msg, wParam, lParam);
                    listener.OnButtonUp(button);
                }
                break;


            case WM_MOUSEWHEEL:
                if (listener != null)
                {
                    listener.OnScroll(EventDecoder.GetMouseWheelDelta(wParam));
                }
                break;

            case WM_MOUSEHWHEEL:
                if (listener != null)
                {
                    listener.OnHScroll(EventDecoder.GetMouseWheelDelta(wParam));
                }
                break;

            // Keyboard
            case WM_CHAR:
                if (listener != null)
                {
                    // TODO: is this always correct?
                    listener.OnChar((char)wParam);
                }
                break;

                // TODO: key down/up

        }
    }
}

