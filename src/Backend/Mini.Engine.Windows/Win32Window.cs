using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;
using Mini.Engine.Windows.Events;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using static Windows.Win32.PInvoke;

namespace Mini.Engine.Windows;

public sealed class Win32Window : IWindowEventListener, IDisposable
{
    private const string WindowSettingsFile = "window.json";

    private bool isCursorDirty = true;
    private Vector2 cursorPosition;

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

        this.Hwnd = hwnd;
    }

    public void Show(bool restorePreviousPosition = true)
    {
        if (restorePreviousPosition)
        {
            RestoreWindowPosition(this.Hwnd);
        }

        ShowWindow(this.Hwnd, SHOW_WINDOW_CMD.SW_NORMAL);

        this.isCursorDirty = true;
    }

    public nint Handle => this.Hwnd;
    internal HWND Hwnd { get; }

    public string Title { get; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public bool IsMinimized { get; private set; }
    public bool HasFocus { get; private set; }

    public void OnSizeChanged(int width, int height)
    {
        this.IsMinimized = width == 0 && height == 0;
        this.Width = width;
        this.Height = height;
    }

    public void OnFocusChanged(bool hasFocus)
    {
        this.HasFocus = hasFocus;
    }

    public void OnDestroyed()
    {
        SerializeWindowPosition(this.Hwnd);
    }

    public void OnMouseMove()
    {
        this.isCursorDirty = true;
    }

    public void OnMouseEnter()
    {
        Win32Application.SetMouseCursor(Cursor.Arrow);
    }

    public void OnMouseLeave()
    {
        Win32Application.SetMouseCursor(Cursor.Default);
    }

    public Vector2 GetCursorPosition()
    {
        if (this.isCursorDirty && GetCursorPos(out var pos) && ScreenToClient(this.Hwnd, ref pos))
        {
            this.cursorPosition = new Vector2(pos.X, pos.Y);
            this.isCursorDirty = true;
        }

        return this.cursorPosition;
    }

    public void SetCursorPosition(Vector2 position)
    {
        var pos = new System.Drawing.Point((int)position.X, (int)position.Y);
        ClientToScreen(this.Hwnd, ref pos);
        SetCursorPos(pos.X, pos.Y);
    }

    public void Dispose()
    {
        DestroyWindow(this.Hwnd);
    }

    private static void SerializeWindowPosition(HWND handle)
    {
        try
        {
            var placement = new WINDOWPLACEMENT() { length = (uint)Marshal.SizeOf<WINDOWPLACEMENT>() };
            var success = GetWindowPlacement(handle, ref placement);
            if (success && placement.showCmd == SHOW_WINDOW_CMD.SW_NORMAL)
            {
                var rectangle = new Rectangle(placement.rcNormalPosition.X, placement.rcNormalPosition.Y, placement.rcNormalPosition.Width, placement.rcNormalPosition.Height);
                using var stream = File.Create(WindowSettingsFile);
                var serializer = new DataContractJsonSerializer(typeof(Rectangle));
                serializer.WriteObject(stream, rectangle);
            }
        }
        catch
        {

        }
    }

    private static void RestoreWindowPosition(HWND hwnd)
    {
        try
        {
            using var stream = File.OpenRead(WindowSettingsFile);
            var deserializer = new DataContractJsonSerializer(typeof(Rectangle));
            var rectangle = (Rectangle?)deserializer.ReadObject(stream) ?? throw new IOException();
            var pos = new WINDOWPLACEMENT()
            {
                length = (uint)Marshal.SizeOf<WINDOWPLACEMENT>(),
                showCmd = SHOW_WINDOW_CMD.SW_NORMAL,
                ptMinPosition = new Point(-1, -1),
                ptMaxPosition = new Point(-1, -1),
                rcNormalPosition = new RECT(rectangle)
            };
            SetWindowPlacement(hwnd, pos);
        }
        catch
        {

        }
    }
}
