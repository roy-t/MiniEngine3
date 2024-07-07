using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using static Windows.Win32.PInvoke;

namespace Mini.Engine.Windows.Events;

public sealed class EventProcessor
{
    private class WindowState(HWND target, IWindowEventListener listener)
    {
        public HWND Target { get; } = target;
        public IWindowEventListener Listener { get; } = listener;
        public bool IsMouseCaptured { get; set; }
        public bool HasMouseEntered { get; set; }
    }

    private class InputState(HWND target, IInputEventListener listener)
    {
        public HWND Target { get; } = target;
        public IInputEventListener Listener { get; } = listener;
    }

    private readonly List<WindowState> WindowEventListeners;
    private readonly List<InputState> InputEventListeners;

    public EventProcessor()
    {
        this.WindowEventListeners = [];
        this.InputEventListeners = [];
    }

    public void Register(Win32Window window, IWindowEventListener listener)
    {
        var state = new WindowState(window.Hwnd, listener);
        this.WindowEventListeners.Add(state);
    }

    public void Register(Win32Window window, IInputEventListener listener)
    {
        var state = new InputState(window.Hwnd, listener);
        this.InputEventListeners.Add(state);
    }

    internal void FireWindowEvents(HWND hWnd, uint msg, UIntPtr wParam, IntPtr lParam)
    {
        for (var i = this.WindowEventListeners.Count - 1; i >= 0; i--)
        {
            var window = this.WindowEventListeners[i];
            if (window.Target != hWnd) { continue; }

            switch (msg)
            {
                case WM_SIZE:
                    var lp = lParam.ToInt32();
                    var width = EventDecoder.Loword(lp);
                    var height = EventDecoder.Hiword(lp);

                    switch (wParam.ToUInt32())
                    {
                        case SIZE_RESTORED:
                        case SIZE_MAXIMIZED:
                        case SIZE_MINIMIZED:
                            window.Listener.OnSizeChanged(width, height);
                            break;
                    }
                    break;

                case WM_SETFOCUS:
                    window.Listener.OnFocusChanged(true);
                    break;

                case WM_KILLFOCUS:
                    window.Listener.OnFocusChanged(false);
                    break;

                case WM_ACTIVATE:
                    window.Listener.OnFocusChanged(EventDecoder.Loword((int)wParam) != 0);
                    break;

                case WM_DESTROY:
                    window.Listener.OnDestroyed();
                    this.WindowEventListeners.RemoveAt(i);
                    break;

                case WM_MOUSEMOVE:
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
                    break;

                case WM_MOUSELEAVE:
                    window.Listener.OnMouseMove();
                    window.Listener.OnMouseLeave();
                    window.HasMouseEntered = false;

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
                    SetCapture(hWnd);
                    window.IsMouseCaptured = true;

                    break;

                case WM_LBUTTONUP:
                case WM_RBUTTONUP:
                case WM_MBUTTONUP:
                case WM_XBUTTONUP:
                    ReleaseCapture();
                    window.IsMouseCaptured = false;

                    break;

            }
        }

        for (var i = this.InputEventListeners.Count - 1; i >= 0; i--)
        {
            var input = this.InputEventListeners[i];
            if (input.Target != hWnd) { continue; }

            switch (msg)
            {
                case WM_MOUSEWHEEL:
                    input.Listener.OnScroll(EventDecoder.GetMouseWheelDelta(wParam));
                    break;

                case WM_MOUSEHWHEEL:
                    input.Listener.OnHScroll(EventDecoder.GetMouseWheelDelta(wParam));
                    break;

                case WM_CHAR:
                    input.Listener.OnChar((char)wParam);
                    break;

                case WM_KEYDOWN:
                case WM_SYSKEYDOWN:
                    input.Listener.OnKeyDown(EventDecoder.GetKeyCode(wParam));
                    break;

                case WM_KEYUP:
                case WM_SYSKEYUP:
                    input.Listener.OnKeyUp(EventDecoder.GetKeyCode(wParam));
                    break;

                case WM_LBUTTONDOWN:
                case WM_LBUTTONDBLCLK:
                case WM_RBUTTONDOWN:
                case WM_RBUTTONDBLCLK:
                case WM_MBUTTONDOWN:
                case WM_MBUTTONDBLCLK:
                case WM_XBUTTONDOWN:
                case WM_XBUTTONDBLCLK:
                    input.Listener.OnButtonDown(EventDecoder.GetMouseButton(msg, wParam, lParam));
                    break;

                case WM_LBUTTONUP:
                case WM_RBUTTONUP:
                case WM_MBUTTONUP:
                case WM_XBUTTONUP:
                    input.Listener.OnButtonUp(EventDecoder.GetMouseButton(msg, wParam, lParam));
                    break;
            }
        }
    }
}

