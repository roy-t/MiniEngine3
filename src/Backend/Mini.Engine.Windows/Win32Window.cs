using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using static Windows.Win32.PInvoke;

namespace Mini.Engine.Windows;

public sealed class Win32Window : IDisposable
{
    private const string WindowSettingsFile = "window.json";

    internal unsafe Win32Window(string title)
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
    }

    public void Show(bool restorePreviousPosition)
    {
        var show = SHOW_WINDOW_CMD.SW_NORMAL;
        if (restorePreviousPosition && TryDeserializeWindowPosition(out var pos))
        {
            SetWindowPlacement(this.Handle, pos);
            show = pos.showCmd;
        }

        ShowWindow(this.Handle, show);
    }

    public string Title { get; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public HWND Handle { get; private set; }
    public bool IsMinimized { get; private set; }
    public bool HasFocus { get; private set; }

    internal void OnSizeChanged(int width, int height)
    {
        this.IsMinimized = width == 0 && height == 0;
        this.Width = width;
        this.Height = height;
    }

    internal void OnFocusChanged(bool hasFocus)
    {
        this.HasFocus = hasFocus;
    }

    internal void OnDestroyed()
    {
        TrySerializeWindowPosition(this.Handle);
    }

    public void Dispose()
    {
        DestroyWindow(this.Handle);
    }

    private static void TrySerializeWindowPosition(HWND handle)
    {
        try
        {
            var placement = new WINDOWPLACEMENT() { length = (uint)Marshal.SizeOf<WINDOWPLACEMENT>() };
            var success = GetWindowPlacement(handle, ref placement);
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
