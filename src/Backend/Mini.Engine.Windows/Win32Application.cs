using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mini.Engine.Windows.Events;
using static Windows.Win32.PInvoke;
using static global::Windows.Win32.Constants;
using Windows.Win32.UI.WindowsAndMessaging;
using Windows.Win32.Foundation;

namespace Mini.Engine.Windows;

public static class Win32Application
{
    public static readonly WindowEvents WindowEvents = new WindowEvents();
    public static readonly RawEvents RawEvents = new RawEvents();

    public static unsafe Win32Window Initialize(string title)
    {
#nullable disable
        var moduleHandle = GetModuleHandle((string)null);
#nullable restore
        var cursor = LoadCursor((HINSTANCE)IntPtr.Zero, IDC_ARROW);
        fixed (char* ptrClassName = "WndClass")
        {
            var wndClass = new WNDCLASSEXW
            {
                cbSize = (uint)Marshal.SizeOf<WNDCLASSEXW>(),
                style = WNDCLASS_STYLES.CS_HREDRAW | WNDCLASS_STYLES.CS_VREDRAW | WNDCLASS_STYLES.CS_OWNDC,
                lpfnWndProc = &WndProc,
                hInstance = (HINSTANCE)moduleHandle.DangerousGetHandle(),
                hCursor = cursor,
                hbrBackground = (global::Windows.Win32.Graphics.Gdi.HBRUSH)IntPtr.Zero,
                hIcon = (HICON)IntPtr.Zero,
                lpszClassName = new PCWSTR(ptrClassName)
            };

            RegisterClassEx(wndClass);
        }
        
        return new Win32Window(title, WindowEvents);
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
        while (PeekMessage(out var msg, (global::Windows.Win32.Foundation.HWND)IntPtr.Zero, 0, 0, PEEK_MESSAGE_REMOVE_TYPE.PM_REMOVE))
        {
            TranslateMessage(msg);
            DispatchMessage(msg);            
            @continue = @continue && (msg.message != WM_QUIT);
        }

        return @continue;
    }

    [UnmanagedCallersOnly(CallConvs = new[] {typeof(CallConvStdcall)})]
    public static global::Windows.Win32.Foundation.LRESULT WndProc(global::Windows.Win32.Foundation.HWND hWnd, uint msg, global::Windows.Win32.Foundation.WPARAM wParam, global::Windows.Win32.Foundation.LPARAM lParam)
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

        return DefWindowProc(hWnd, msg, wParam, lParam);
    }
}
