using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mini.Engine.Windows.Events;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using static Windows.Win32.PInvoke;

namespace Mini.Engine.Windows;

public static class Win32Application
{
    private static readonly EventProcessor EventProcessor = new EventProcessor();

    public static unsafe Win32Window Initialize(string title)
    {
        var moduleHandle = GetModuleHandle(string.Empty);
        fixed (char* ptrClassName = "WndClass")
        {
            var cursor = LoadCursor((HINSTANCE)IntPtr.Zero, IDC_ARROW);

            var wndClass = new WNDCLASSEXW
            {
                cbSize = (uint)Marshal.SizeOf<WNDCLASSEXW>(),
                style = WNDCLASS_STYLES.CS_HREDRAW | WNDCLASS_STYLES.CS_VREDRAW | WNDCLASS_STYLES.CS_OWNDC,
                lpfnWndProc = &WndProc,
                hInstance = (HINSTANCE)moduleHandle.DangerousGetHandle(),
                hCursor = HCURSOR.Null,
                hbrBackground = global::Windows.Win32.Graphics.Gdi.HBRUSH.Null,
                hIcon = HICON.Null,
                lpszClassName = new PCWSTR(ptrClassName),
            };

            RegisterClassEx(wndClass);
        }

        var window = new Win32Window(title);
        EventProcessor.Register(window, window);
        window.Show();

        SetMouseCursor(Cursor.Arrow);

        return window;
    }

    public static void RegisterInputEventListener(Win32Window window, IInputEventListener listener)
    {
        EventProcessor.Register(window, listener);
    }

    public static void SetMouseCursor(Cursor cursor)
    {
        if (cursor == Cursor.Default)
        {
            SetCursor(null);
        }

        unsafe
        {
            PCWSTR resource = (char*)(int)cursor;
            var hCursor = LoadCursor((HINSTANCE)IntPtr.Zero, resource);
            SetCursor(hCursor);
        }
    }

    public static bool PumpMessages()
    {
        var @continue = true;
        while (PeekMessage(out var msg, (HWND)IntPtr.Zero, 0, 0, PEEK_MESSAGE_REMOVE_TYPE.PM_REMOVE))
        {
            TranslateMessage(msg);
            DispatchMessage(msg);
            @continue = @continue && (msg.message != WM_QUIT);
        }

        return @continue;
    }



    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
    internal static LRESULT WndProc(HWND hWnd, uint msg, WPARAM wParam, LPARAM lParam)
    {
        EventProcessor.FireWindowEvents(hWnd, msg, wParam, lParam);
        switch (msg)
        {
            case WM_DESTROY:
                PostQuitMessage(0);
                break;
        }

        return DefWindowProc(hWnd, msg, wParam, lParam);
    }
}
