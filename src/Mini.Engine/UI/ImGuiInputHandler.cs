using ImGuiNET;
using Mini.Engine.Windows;
using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;
using static Windows.Win32.UI.Input.KeyboardAndMouse.VIRTUAL_KEY;

namespace Mini.Engine.UI;

internal sealed class ImGuiInputHandler
{
    private const int WHEEL_DELTA = 120;

    private readonly HWND HWND;
    private ImGuiMouseCursor lastCursor;

    public ImGuiInputHandler(HWND hwnd)
    {
        this.HWND = hwnd;
        InitKeyMap();
        Win32Application.RawEvents.OnEvent += (o, e) => this.ProcessMessage(e.Msg, e.WParam, e.LParam);
    }

    private static void InitKeyMap()
    {
        var io = ImGui.GetIO();

        io.KeyMap[(int)ImGuiKey.Tab] = (int)VK_TAB;
        io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)VK_LEFT;
        io.KeyMap[(int)ImGuiKey.RightArrow] = (int)VK_RIGHT;
        io.KeyMap[(int)ImGuiKey.UpArrow] = (int)VK_UP;
        io.KeyMap[(int)ImGuiKey.DownArrow] = (int)VK_DOWN;
        io.KeyMap[(int)ImGuiKey.PageUp] = (int)VK_PRIOR;
        io.KeyMap[(int)ImGuiKey.PageDown] = (int)VK_NEXT;
        io.KeyMap[(int)ImGuiKey.Home] = (int)VK_HOME;
        io.KeyMap[(int)ImGuiKey.End] = (int)VK_END;
        io.KeyMap[(int)ImGuiKey.Insert] = (int)VK_INSERT;
        io.KeyMap[(int)ImGuiKey.Delete] = (int)VK_DELETE;
        io.KeyMap[(int)ImGuiKey.Backspace] = (int)VK_BACK;
        io.KeyMap[(int)ImGuiKey.Space] = (int)VK_SPACE;
        io.KeyMap[(int)ImGuiKey.Enter] = (int)VK_RETURN;
        io.KeyMap[(int)ImGuiKey.Escape] = (int)VK_ESCAPE;
        io.KeyMap[(int)ImGuiKey.KeypadEnter] = (int)VK_RETURN;
        io.KeyMap[(int)ImGuiKey.A] = 'A';
        io.KeyMap[(int)ImGuiKey.C] = 'C';
        io.KeyMap[(int)ImGuiKey.V] = 'V';
        io.KeyMap[(int)ImGuiKey.X] = 'X';
        io.KeyMap[(int)ImGuiKey.Y] = 'Y';
        io.KeyMap[(int)ImGuiKey.Z] = 'Z';
    }

    public void Update()
    {
        this.UpdateKeyModifiers();
        this.UpdateMousePosition();

        var mouseCursor = ImGui.GetIO().MouseDrawCursor ? ImGuiMouseCursor.None : ImGui.GetMouseCursor();
        if (mouseCursor != this.lastCursor)
        {
            this.lastCursor = mouseCursor;
            this.UpdateMouseCursor();
        }
    }

    private void UpdateKeyModifiers()
    {
        var io = ImGui.GetIO();
        io.KeyCtrl = (GetKeyState((int)VK_CONTROL) & 0x8000) != 0;
        io.KeyShift = (GetKeyState((int)VK_SHIFT) & 0x8000) != 0;
        io.KeyAlt = (GetKeyState((int)VK_MENU) & 0x8000) != 0;
        io.KeySuper = false;
    }

    public bool UpdateMouseCursor()
    {
        var io = ImGui.GetIO();
        if ((io.ConfigFlags & ImGuiConfigFlags.NoMouseCursorChange) != 0)
        {
            return false;
        }

        var requestedcursor = ImGui.GetMouseCursor();
        if (requestedcursor == ImGuiMouseCursor.None || io.MouseDrawCursor)
        {
            SetCursor(null);
        }
        else
        {
            var cursor = IDC_ARROW;
            switch (requestedcursor)
            {
                case ImGuiMouseCursor.Arrow: cursor = IDC_ARROW; break;
                case ImGuiMouseCursor.TextInput: cursor = IDC_IBEAM; break;
                case ImGuiMouseCursor.ResizeAll: cursor = IDC_SIZEALL; break;
                case ImGuiMouseCursor.ResizeEW: cursor = IDC_SIZEWE; break;
                case ImGuiMouseCursor.ResizeNS: cursor = IDC_SIZENS; break;
                case ImGuiMouseCursor.ResizeNESW: cursor = IDC_SIZENESW; break;
                case ImGuiMouseCursor.ResizeNWSE: cursor = IDC_SIZENWSE; break;
                case ImGuiMouseCursor.Hand: cursor = IDC_HAND; break;
                case ImGuiMouseCursor.NotAllowed: cursor = IDC_NO; break;
            }

            var hCursor = LoadCursor((HINSTANCE)IntPtr.Zero, cursor);
            SetCursor(hCursor);
        }

        return true;
    }

    private void UpdateMousePosition()
    {
        var io = ImGui.GetIO();

        if (io.WantSetMousePos)
        {
            var pos = new System.Drawing.Point((int)io.MousePos.X, (int)io.MousePos.Y);
            ClientToScreen(this.HWND, ref pos);
            SetCursorPos(pos.X, pos.Y);
        }

        var foregroundWindow = GetForegroundWindow();
        if (foregroundWindow == this.HWND || IsChild(foregroundWindow, this.HWND))
        {
            if (GetCursorPos(out var pos) && ScreenToClient(this.HWND, ref pos))
            {
                io.MousePos = new System.Numerics.Vector2(pos.X, pos.Y);
            }
        }
    }

    private bool ProcessMessage(uint msg, UIntPtr wParam, IntPtr lParam)
    {
        if (ImGui.GetCurrentContext() == IntPtr.Zero)
        {
            return false;
        }

        var io = ImGui.GetIO();

        switch (msg)
        {
            case WM_LBUTTONDOWN:
            case WM_LBUTTONDBLCLK:
            case WM_RBUTTONDOWN:
            case WM_RBUTTONDBLCLK:
            case WM_MBUTTONDOWN:
            case WM_MBUTTONDBLCLK:
            case WM_XBUTTONDOWN:
            case WM_XBUTTONDBLCLK:
                {
                    int button = 0;
                    if (msg == WM_LBUTTONDOWN || msg == WM_LBUTTONDBLCLK) { button = 0; }
                    if (msg == WM_RBUTTONDOWN || msg == WM_RBUTTONDBLCLK) { button = 1; }
                    if (msg == WM_MBUTTONDOWN || msg == WM_MBUTTONDBLCLK) { button = 2; }
                    if (msg == WM_XBUTTONDOWN || msg == WM_XBUTTONDBLCLK) { button = (GET_XBUTTON_WPARAM(wParam) == 1) ? 3 : 4; }
                    if (!ImGui.IsAnyMouseDown() && GetCapture() == IntPtr.Zero)
                    {
                        SetCapture(this.HWND);
                    }

                    io.MouseDown[button] = true;
                    return false;
                }
            case WM_LBUTTONUP:
            case WM_RBUTTONUP:
            case WM_MBUTTONUP:
            case WM_XBUTTONUP:
                {
                    int button = 0;
                    if (msg == WM_LBUTTONUP) { button = 0; }
                    if (msg == WM_RBUTTONUP) { button = 1; }
                    if (msg == WM_MBUTTONUP) { button = 2; }
                    if (msg == WM_XBUTTONUP) { button = (GET_XBUTTON_WPARAM(wParam) == 1) ? 3 : 4; }
                    io.MouseDown[button] = false;
                    if (!ImGui.IsAnyMouseDown() && GetCapture() == this.HWND)
                    {
                        ReleaseCapture();
                    }

                    return false;
                }
            case WM_MOUSEWHEEL:
                io.MouseWheel += GET_WHEEL_DELTA_WPARAM(wParam) / WHEEL_DELTA;
                return false;
            case WM_MOUSEHWHEEL:
                io.MouseWheelH += GET_WHEEL_DELTA_WPARAM(wParam) / WHEEL_DELTA;
                return false;
            case WM_KEYDOWN:
            case WM_SYSKEYDOWN:
                if ((ulong)wParam < 256)
                {
                    io.KeysDown[(int)wParam] = true;
                }

                return false;
            case WM_KEYUP:
            case WM_SYSKEYUP:
                if ((ulong)wParam < 256)
                {
                    io.KeysDown[(int)wParam] = false;
                }

                return false;
            case WM_CHAR:
                io.AddInputCharacter((uint)wParam);
                return false;
            case WM_SETCURSOR:
                var low = Loword((int)lParam);
                if (low == 1 && this.UpdateMouseCursor())
                {
                    return true;
                }

                return false;
        }
        return false;
    }

    private static int GET_WHEEL_DELTA_WPARAM(UIntPtr wParam)
    {
        return Hiword((int)wParam);
    }

    private static int GET_XBUTTON_WPARAM(UIntPtr wParam)
    {
        return Hiword((int)wParam);
    }

    private static int Loword(int number)
    {
        return number & 0x0000FFFF;
    }

    private static int Hiword(int number)
    {
        return number >> 16;
    }
}
