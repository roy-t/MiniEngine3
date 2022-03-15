using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;
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
    private const string WindowSettingsFile = "window.json";

    internal Win32Window(string title, WindowEvents windowEvents)
    {
        this.Title = title;

        var screenWidth = GetSystemMetrics(SystemMetrics.SM_CXSCREEN);
        var screenHeight = GetSystemMetrics(SystemMetrics.SM_CYSCREEN);
        var left = screenWidth / 4;
        var top = screenHeight / 4;
        var right = left * 3;
        var bottom = top * 3;

        var windowRect = new RawRect(left, top, right, bottom);        

        var style = WS_OVERLAPPEDWINDOW;
        var styleEx = WS_EX_APPWINDOW | WS_EX_WINDOWEDGE;
       
        AdjustWindowRectEx(ref windowRect, style, false, styleEx);

        this.Width = windowRect.Right - windowRect.Left;
        this.Height = windowRect.Bottom - windowRect.Top;

        var hwnd = CreateWindowEx(
            styleEx, "WndClass", this.Title, (int)style,
            windowRect.Left, windowRect.Top, this.Width, this.Height,
            IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

        this.Handle = hwnd;
        
        windowEvents.OnResize += (o, e) =>
        {
            this.IsMinimized = e.Width == 0 && e.Height == 0;
            this.Width = e.Width;
            this.Height = e.Height;
        };

        windowEvents.OnFocus += (o, e) => this.HasFocus = e;
        windowEvents.OnDestroy += (o, e) => TrySerializeWindowPosition(this.Handle);

        var show = ShowWindowCommand.Normal;
        if (TryDeserializeWindowPosition(out var pos))
        {
            SetWindowPlacement((global::Windows.Win32.Foundation.HWND)this.Handle, pos);
            if (pos.showCmd == SHOW_WINDOW_CMD.SW_MAXIMIZE)
            {
                show = ShowWindowCommand.Maximize;
            }
        }

        ShowWindow(this.Handle, show);
    }

    public string Title { get; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public IntPtr Handle { get; private set; }
    public bool IsMinimized { get; private set; }
    public bool HasFocus { get; private set; }

    public void Dispose()
    {
        DestroyWindow(this.Handle);
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
