using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mini.Engine.Windows.Events;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using static Windows.Win32.PInvoke;

namespace Mini.Engine.Windows;

public static class Win32Application
{
    public static readonly RawEvents RawEvents = new RawEvents();

    private static readonly WindowEvents WindowEvents = new WindowEvents();


    public static unsafe Win32Window Initialize(string title)
    {
        var moduleHandle = GetModuleHandle(string.Empty);
        fixed (char* ptrClassName = "WndClass")
        {
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
        WindowEvents.Register(window);
        window.Show(true);
        return window;
    }

    public static void RegisterMessageListener(uint message, Action<UIntPtr, IntPtr> handler)
    {
        RawEvents.OnEvent += (o, e) =>
        {
            if (e.Msg == message)
            {
                handler(e.WParam, e.LParam);
            }
        };
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
    public static LRESULT WndProc(HWND hWnd, uint msg, WPARAM wParam, LPARAM lParam)
    {
        // TODO: ideally we never want to expose these events, right now its necessary for input
        // but we should replace it with input system similar to what ImGui uses

        // TODO: maybe we can move the input classes here and make ImGui use RawInputController?
        RawEvents.FireWindowEvents(hWnd, msg, wParam, lParam);
        WindowEvents.FireWindowEvents(hWnd, msg, wParam, lParam);
        switch (msg)
        {
            case WM_DESTROY:
                PostQuitMessage(0);
                break;
        }


        // TODO: move the stuff from the IMGUI WndProc here, taking care of capturing and uncapturing the mouse
        // add a method for setting the cursor
        // if we do everything well we no longer have to expose the method below and the ImGuiInputHandler can do everything
        // via an api with this application!

        return DefWindowProc(hWnd, msg, wParam, lParam);
    }
}
