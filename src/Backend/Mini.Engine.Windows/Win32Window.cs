using System;
using System.IO;
using System.Runtime.InteropServices;
using Mini.Engine.Windows.Events;
using Vortice;
using Vortice.Win32;
using Windows.Win32.UI.WindowsAndMessaging;
using static Vortice.Win32.User32;
using static Vortice.Win32.WindowExStyles;
using static Vortice.Win32.WindowStyles;
using static Windows.Win32.PInvoke;

namespace Mini.Engine.Windows;

public sealed class Win32Window : IDisposable
{
    private const string WindowSettingsFile = "window.ini";

    internal Win32Window(string title, int width, int height, WindowEvents windowEvents)
    {
        this.Title = title;
        this.Width = width;
        this.Height = height;

        var screenWidth = GetSystemMetrics(SystemMetrics.SM_CXSCREEN);
        var screenHeight = GetSystemMetrics(SystemMetrics.SM_CYSCREEN);
        var x = (screenWidth - this.Width) / 2;
        var y = (screenHeight - this.Height) / 2;

        if (TryDeserializeWindowPosition(out var pos))
        {
            x = pos.Left;
            y = pos.Top;
        }

        var style = WS_OVERLAPPEDWINDOW;
        var styleEx = WS_EX_APPWINDOW | WS_EX_WINDOWEDGE;

        var windowRect = new RawRect(0, 0, this.Width, this.Height);
        AdjustWindowRectEx(ref windowRect, style, false, styleEx);

        var windowWidth = windowRect.Right - windowRect.Left;
        var windowHeight = windowRect.Bottom - windowRect.Top;

        var hwnd = CreateWindowEx(
            styleEx, "WndClass", this.Title, (int)style,
            x, y, windowWidth, windowHeight,
            IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

        this.Handle = hwnd;

        windowEvents.OnResize += (o, e) =>
        {
            this.IsMinimized = e.Width == 0 && e.Height == 0;
        };

        windowEvents.OnFocus += (o, e) =>
        {
            this.HasFocus = e;
        };

        windowEvents.OnDestroy += (o, e) =>
        {
            this.TrySerializeWindowPosition();
        };
    }

    private void TrySerializeWindowPosition()
    {
        try
        {
            var placement = new WINDOWPLACEMENT() { length = (uint)Marshal.SizeOf<WINDOWPLACEMENT>() };
            var success = GetWindowPlacement((global::Windows.Win32.Foundation.HWND)this.Handle, ref placement);
            if (success)
            {
                using var stream = File.CreateText(WindowSettingsFile);
                stream.WriteLine($"{placement.rcNormalPosition.left};{placement.rcNormalPosition.top};{placement.rcNormalPosition.right};{placement.rcNormalPosition.bottom}");
            }
        }
        catch
        {

        }
    }

    private static bool TryDeserializeWindowPosition(out RawRect position)
    {
        try
        {
            var text = File.ReadAllText(WindowSettingsFile);
            var split = text.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var left = int.Parse(split[0]);
            var top = int.Parse(split[1]);
            var right = int.Parse(split[2]);
            var bottom = int.Parse(split[3]);

            position = new RawRect(left, top, right, bottom);

            return true;
        }
        catch
        {
            position = default;
            return false;
        }
    }

    public string Title { get; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public IntPtr Handle { get; private set; }
    public bool IsMinimized { get; private set; }
    public bool HasFocus { get; private set; }

    public void Show()
    {
        ShowWindow(this.Handle, ShowWindowCommand.Normal);
    }

    public void Dispose()
    {


        DestroyWindow(this.Handle);
    }
}
