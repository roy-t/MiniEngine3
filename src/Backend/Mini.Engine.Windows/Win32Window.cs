using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;
using Mini.Engine.Windows.Events;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using static Windows.Win32.PInvoke;

namespace Mini.Engine.Windows;

public sealed class Win32Window : IDisposable
{
    private const string WindowSettingsFile = "window.json";

    internal unsafe Win32Window(string title, WindowEvents windowEvents)
    {
        this.Title = title;

        var screenWidth = GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXSCREEN);
        var screenHeight = GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CYSCREEN);

        var windowRect = new RECT()
        {
            left = screenWidth / 4,
            top = screenHeight / 4,
            right = (screenWidth / 4) * 3,
            bottom = (screenHeight / 4) * 3,
        };

        var style = WINDOW_STYLE.WS_OVERLAPPEDWINDOW;
        var styleEx = WINDOW_EX_STYLE.WS_EX_APPWINDOW | WINDOW_EX_STYLE.WS_EX_WINDOWEDGE;

        AdjustWindowRectEx(ref windowRect, style, false, styleEx);

        this.Width = windowRect.right - windowRect.left;
        this.Height = windowRect.bottom - windowRect.top;

        var hwnd = CreateWindowEx(
            styleEx, "WndClass", this.Title, style,
            windowRect.left, windowRect.top, this.Width, this.Height,
            (HWND)IntPtr.Zero, null, null, null);

        this.Handle = hwnd;

        windowEvents.OnResize += (o, e) =>
        {
            this.IsMinimized = e.Width == 0 && e.Height == 0;
            this.Width = e.Width;
            this.Height = e.Height;
        };

        windowEvents.OnFocus += (o, e) => this.HasFocus = e;
        windowEvents.OnDestroy += (o, e) => TrySerializeWindowPosition(this.Handle);

        var show = SHOW_WINDOW_CMD.SW_NORMAL;
        if (TryDeserializeWindowPosition(out var pos))
        {
            SetWindowPlacement((HWND)this.Handle, pos);
            show = pos.showCmd;
        }

        ShowWindow((HWND)this.Handle, show);
    }

    public string Title { get; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public IntPtr Handle { get; private set; }
    public bool IsMinimized { get; private set; }
    public bool HasFocus { get; private set; }

    public void Dispose()
    {
        DestroyWindow((HWND)this.Handle);
    }

    private static void TrySerializeWindowPosition(IntPtr handle)
    {
        try
        {
            var placement = new WINDOWPLACEMENT() { length = (uint)Marshal.SizeOf<WINDOWPLACEMENT>() };
            var success = GetWindowPlacement((global::Windows.Win32.Foundation.HWND)handle, ref placement);
            if (success)
            {
                using var stream = File.Create(WindowSettingsFile);
                var serializer = new DataContractJsonSerializer(typeof(WINDOWPLACEMENT));
                serializer.WriteObject(stream, placement);
            }
        }
        catch
        {

        }
    }

    private static bool TryDeserializeWindowPosition(out WINDOWPLACEMENT placement)
    {
        try
        {
            using var stream = File.OpenRead(WindowSettingsFile);
            var deserializer = new DataContractJsonSerializer(typeof(WINDOWPLACEMENT));
            placement = (WINDOWPLACEMENT?)deserializer.ReadObject(stream) ?? throw new IOException();
            return true;
        }
        catch
        {
            placement = default;
            return false;
        }
    }
}
