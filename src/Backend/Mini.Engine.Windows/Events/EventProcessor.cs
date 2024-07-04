using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using static Windows.Win32.PInvoke;

namespace Mini.Engine.Windows.Events;

public sealed class EventProcessor
{
    private class WindowState(IWindowEventListener listener)
    {
        public IWindowEventListener Listener { get; set; } = listener;
        public bool IsMouseCaptured { get; set; }
        public bool HasMouseEntered { get; set; }
    }

    private readonly Dictionary<HWND, WindowState> WindowEventListeners;
    private readonly Dictionary<HWND, IInputEventListener> InputEventListeners;

    public EventProcessor()
    {
        this.WindowEventListeners = new Dictionary<HWND, WindowState>();
        this.InputEventListeners = new Dictionary<HWND, IInputEventListener>();
    }

    public void Register(Win32Window window)
    {
        var state = new WindowState(window);
        this.WindowEventListeners.Add(window.Handle, state);
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
                        window?.Listener.OnSizeChanged(width, height);
                        break;
                }
                break;

            case WM_SETFOCUS:
                window?.Listener.OnFocusChanged(true);
                break;

            case WM_KILLFOCUS:
                window?.Listener.OnFocusChanged(false);
                break;

            case WM_ACTIVATE:
                window?.Listener.OnFocusChanged(EventDecoder.Loword((int)wParam) != 0);
                break;

            case WM_DESTROY:
                if (window != null)
                {
                    window.Listener.OnDestroyed();
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
                if (window != null && !window.IsMouseCaptured)
                {
                    SetCapture(hWnd);
                    window.IsMouseCaptured = true;
                }

                listener?.OnButtonDown(EventDecoder.GetMouseButton(msg, wParam, lParam));
                break;

            case WM_LBUTTONUP:
            case WM_RBUTTONUP:
            case WM_MBUTTONUP:
            case WM_XBUTTONUP:
                if (window != null && window.IsMouseCaptured)
                {
                    ReleaseCapture();
                    window.IsMouseCaptured = false;
                }

                listener?.OnButtonUp(EventDecoder.GetMouseButton(msg, wParam, lParam));
                break;


            case WM_MOUSEWHEEL:
                listener?.OnScroll(EventDecoder.GetMouseWheelDelta(wParam));
                break;

            case WM_MOUSEHWHEEL:
                listener?.OnHScroll(EventDecoder.GetMouseWheelDelta(wParam));
                break;

            case WM_MOUSEMOVE:
                if (window != null)
                {
                    if (!window.HasMouseEntered)
                    {
                        unsafe
                        {
                            var tme = new TRACKMOUSEEVENT()
                            {
                                cbSize = (uint)Marshal.SizeOf<TRACKMOUSEEVENT>(),
                                dwFlags = TRACKMOUSEEVENT_FLAGS.TME_LEAVE,
                                hwndTrack = hWnd,
                            };
                            TrackMouseEvent(ref tme);
                        }

                        window.Listener.OnMouseEnter();
                        window.HasMouseEntered = true;
                    }
                    window.Listener.OnMouseMove();
                }
                break;

            case WM_MOUSELEAVE:
                if (window != null)
                {
                    window.Listener.OnMouseMove();
                    window.Listener.OnMouseLeave();
                    window.HasMouseEntered = false;
                }
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
        }
    }
}

