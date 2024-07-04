using static Windows.Win32.PInvoke;

namespace Mini.Engine.Windows.Events;
internal static class EventDecoder
{
    private const int WheelDelta = 120;

    public static MouseButton GetMouseButton(uint msg, UIntPtr wParam, IntPtr lParam)
    {
        return msg switch
        {
            WM_LBUTTONDOWN or WM_LBUTTONDBLCLK or WM_LBUTTONUP => MouseButton.Left,
            WM_RBUTTONDOWN or WM_RBUTTONDBLCLK or WM_RBUTTONUP => MouseButton.Right,
            WM_MBUTTONDOWN or WM_MBUTTONDBLCLK or WM_MBUTTONUP => MouseButton.Middle,
            WM_XBUTTONDOWN or WM_XBUTTONDBLCLK or WM_XBUTTONUP => GetXButtonWParam(wParam) == 1
                                ? MouseButton.Four
                                : MouseButton.Five,
            _ => throw new ArgumentOutOfRangeException(nameof(msg)),
        };
    }

    // Note: that Microsoft defines one 'notch' on the scroll wheel as 120 units of movements
    public static int GetMouseWheelDelta(UIntPtr wParam)
    {
        return GetWheelDelta(wParam) / WheelDelta;
    }

    public static VirtualKeyCode GetKeyCode(UIntPtr wParam)
    {
        return new VirtualKeyCode((byte)wParam);
    }

    private static int GetXButtonWParam(UIntPtr wParam)
    {
        return Hiword((int)wParam);
    }

    private static int GetWheelDelta(UIntPtr wParam)
    {
        return Hiword((int)wParam);
    }

    public static int Loword(int number)
    {
        return number & 0x0000FFFF;
    }

    public static int Hiword(int number)
    {
        return number >> 16;
    }
}
