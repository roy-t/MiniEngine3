using ImGuiNET;
using Mini.Engine.Windows;
using Mini.Engine.Windows.Events;

namespace Mini.Engine.UI;
// TODO: the input handler example from dearimgui is much more complex:
// https://github.com/ocornut/imgui/blob/master/backends/imgui_impl_win32.cpp
public sealed class ImGuiInputEventListener : IInputEventListener
{
    private const float WHEEL_DELTA = 120.0f;

    private readonly Win32Window Window;
    private ImGuiMouseCursor lastCursor;

    public ImGuiInputEventListener(Win32Window window)
    {
        this.Window = window;
    }

    public void Update()
    {
        this.UpdateMousePosition();

        var mouseCursor = ImGui.GetIO().MouseDrawCursor ? ImGuiMouseCursor.None : ImGui.GetMouseCursor();
        if (mouseCursor != this.lastCursor)
        {
            this.lastCursor = mouseCursor;
            UpdateMouseCursor();
        }
    }

    private void UpdateMousePosition()
    {
        var io = ImGui.GetIO();

        if (io.WantSetMousePos)
        {
            this.Window.SetCursorPosition(io.MousePos);
        }

        if (this.Window.HasFocus)
        {
            io.MousePos = this.Window.GetCursorPosition();
        }
    }

    private static void UpdateMouseCursor()
    {
        var io = ImGui.GetIO();
        if ((io.ConfigFlags & ImGuiConfigFlags.NoMouseCursorChange) != 0)
        {
            return;
        }

        var requestedcursor = ImGui.GetMouseCursor();
        if (requestedcursor == ImGuiMouseCursor.None || io.MouseDrawCursor)
        {
            Win32Application.SetMouseCursor(Cursor.Default);
        }
        else
        {
            var cursor = Cursor.Arrow;
            switch (requestedcursor)
            {
                case ImGuiMouseCursor.Arrow: cursor = Cursor.Arrow; break;
                case ImGuiMouseCursor.TextInput: cursor = Cursor.IBeam; break;
                case ImGuiMouseCursor.ResizeAll: cursor = Cursor.SizeAll; break;
                case ImGuiMouseCursor.ResizeEW: cursor = Cursor.SizeWE; break;
                case ImGuiMouseCursor.ResizeNS: cursor = Cursor.SizeNS; break;
                case ImGuiMouseCursor.ResizeNESW: cursor = Cursor.SizeNESW; break;
                case ImGuiMouseCursor.ResizeNWSE: cursor = Cursor.SizeNWSE; break;
                case ImGuiMouseCursor.Hand: cursor = Cursor.Hand; break;
                case ImGuiMouseCursor.NotAllowed: cursor = Cursor.No; break;
            }

            Win32Application.SetMouseCursor(cursor);
        }
    }

    public void OnButtonDown(MouseButton button)
    {
        var io = ImGui.GetIO();
        io.MouseDown[(int)button] = true;
    }

    public void OnButtonUp(MouseButton button)
    {
        var io = ImGui.GetIO();
        io.MouseDown[(int)button] = false;
    }

    public void OnChar(char character)
    {
        var io = ImGui.GetIO();
        io.AddInputCharacter(character);
    }

    public void OnScroll(int delta)
    {
        var io = ImGui.GetIO();
        io.MouseWheel += (delta / WHEEL_DELTA);
    }

    public void OnHScroll(int delta)
    {
        var io = ImGui.GetIO();
        io.MouseWheelH += (delta / WHEEL_DELTA);
    }

    public void OnKeyDown(VirtualKeyCode key)
    {
        var io = ImGui.GetIO();
        io.AddKeyEvent(Map(key), true);

        if (key == VirtualKeyCode.VK_MENU) { io.KeyAlt = true; }
        if (key == VirtualKeyCode.VK_CONTROL) { io.KeyCtrl = true; }
        if (key == VirtualKeyCode.VK_SHIFT) { io.KeyShift = true; }
    }

    public void OnKeyUp(VirtualKeyCode key)
    {
        var io = ImGui.GetIO();
        io.AddKeyEvent(Map(key), false);

        if (key == VirtualKeyCode.VK_MENU) { io.KeyAlt = false; }
        if (key == VirtualKeyCode.VK_CONTROL) { io.KeyCtrl = false; }
        if (key == VirtualKeyCode.VK_SHIFT) { io.KeyShift = false; }
    }

    private static ImGuiKey Map(VirtualKeyCode key)
    {
        return key switch
        {
            VirtualKeyCode.VK_TAB => ImGuiKey.Tab,
            VirtualKeyCode.VK_LEFT => ImGuiKey.LeftArrow,
            VirtualKeyCode.VK_RIGHT => ImGuiKey.RightArrow,
            VirtualKeyCode.VK_UP => ImGuiKey.UpArrow,
            VirtualKeyCode.VK_DOWN => ImGuiKey.DownArrow,
            VirtualKeyCode.VK_PRIOR => ImGuiKey.PageUp,
            VirtualKeyCode.VK_NEXT => ImGuiKey.PageDown,
            VirtualKeyCode.VK_HOME => ImGuiKey.Home,
            VirtualKeyCode.VK_END => ImGuiKey.End,
            VirtualKeyCode.VK_INSERT => ImGuiKey.Insert,
            VirtualKeyCode.VK_DELETE => ImGuiKey.Delete,
            VirtualKeyCode.VK_BACK => ImGuiKey.Backspace,
            VirtualKeyCode.VK_SPACE => ImGuiKey.Space,
            VirtualKeyCode.VK_RETURN => ImGuiKey.Enter,
            VirtualKeyCode.VK_ESCAPE => ImGuiKey.Escape,
            VirtualKeyCode.VK_OEM_7 => ImGuiKey.Apostrophe,
            VirtualKeyCode.VK_OEM_COMMA => ImGuiKey.Comma,
            VirtualKeyCode.VK_OEM_MINUS => ImGuiKey.Minus,
            VirtualKeyCode.VK_OEM_PERIOD => ImGuiKey.Period,
            VirtualKeyCode.VK_OEM_2 => ImGuiKey.Slash,
            VirtualKeyCode.VK_OEM_1 => ImGuiKey.Semicolon,
            VirtualKeyCode.VK_OEM_PLUS => ImGuiKey.Equal,
            VirtualKeyCode.VK_OEM_4 => ImGuiKey.LeftBracket,
            VirtualKeyCode.VK_OEM_5 => ImGuiKey.Backslash,
            VirtualKeyCode.VK_OEM_6 => ImGuiKey.RightBracket,
            VirtualKeyCode.VK_OEM_3 => ImGuiKey.GraveAccent,
            VirtualKeyCode.VK_CAPITAL => ImGuiKey.CapsLock,
            VirtualKeyCode.VK_SCROLL => ImGuiKey.ScrollLock,
            VirtualKeyCode.VK_NUMLOCK => ImGuiKey.NumLock,
            VirtualKeyCode.VK_SNAPSHOT => ImGuiKey.PrintScreen,
            VirtualKeyCode.VK_PAUSE => ImGuiKey.Pause,
            VirtualKeyCode.VK_NUMPAD0 => ImGuiKey.Keypad0,
            VirtualKeyCode.VK_NUMPAD1 => ImGuiKey.Keypad1,
            VirtualKeyCode.VK_NUMPAD2 => ImGuiKey.Keypad2,
            VirtualKeyCode.VK_NUMPAD3 => ImGuiKey.Keypad3,
            VirtualKeyCode.VK_NUMPAD4 => ImGuiKey.Keypad4,
            VirtualKeyCode.VK_NUMPAD5 => ImGuiKey.Keypad5,
            VirtualKeyCode.VK_NUMPAD6 => ImGuiKey.Keypad6,
            VirtualKeyCode.VK_NUMPAD7 => ImGuiKey.Keypad7,
            VirtualKeyCode.VK_NUMPAD8 => ImGuiKey.Keypad8,
            VirtualKeyCode.VK_NUMPAD9 => ImGuiKey.Keypad9,
            VirtualKeyCode.VK_DECIMAL => ImGuiKey.KeypadDecimal,
            VirtualKeyCode.VK_DIVIDE => ImGuiKey.KeypadDivide,
            VirtualKeyCode.VK_MULTIPLY => ImGuiKey.KeypadMultiply,
            VirtualKeyCode.VK_SUBTRACT => ImGuiKey.KeypadSubtract,
            VirtualKeyCode.VK_ADD => ImGuiKey.KeypadAdd,
            VirtualKeyCode.VK_LSHIFT => ImGuiKey.LeftShift,
            VirtualKeyCode.VK_LCONTROL => ImGuiKey.LeftCtrl,
            VirtualKeyCode.VK_LMENU => ImGuiKey.LeftAlt,
            VirtualKeyCode.VK_LWIN => ImGuiKey.LeftSuper,
            VirtualKeyCode.VK_RSHIFT => ImGuiKey.RightShift,
            VirtualKeyCode.VK_RCONTROL => ImGuiKey.RightCtrl,
            VirtualKeyCode.VK_RMENU => ImGuiKey.RightAlt,
            VirtualKeyCode.VK_RWIN => ImGuiKey.RightSuper,
            VirtualKeyCode.VK_APPS => ImGuiKey.Menu,
            VirtualKeyCode.VK_0 => ImGuiKey._0,
            VirtualKeyCode.VK_1 => ImGuiKey._1,
            VirtualKeyCode.VK_2 => ImGuiKey._2,
            VirtualKeyCode.VK_3 => ImGuiKey._3,
            VirtualKeyCode.VK_4 => ImGuiKey._4,
            VirtualKeyCode.VK_5 => ImGuiKey._5,
            VirtualKeyCode.VK_6 => ImGuiKey._6,
            VirtualKeyCode.VK_7 => ImGuiKey._7,
            VirtualKeyCode.VK_8 => ImGuiKey._8,
            VirtualKeyCode.VK_9 => ImGuiKey._9,
            VirtualKeyCode.VK_A => ImGuiKey.A,
            VirtualKeyCode.VK_B => ImGuiKey.B,
            VirtualKeyCode.VK_C => ImGuiKey.C,
            VirtualKeyCode.VK_D => ImGuiKey.D,
            VirtualKeyCode.VK_E => ImGuiKey.E,
            VirtualKeyCode.VK_F => ImGuiKey.F,
            VirtualKeyCode.VK_G => ImGuiKey.G,
            VirtualKeyCode.VK_H => ImGuiKey.H,
            VirtualKeyCode.VK_I => ImGuiKey.I,
            VirtualKeyCode.VK_J => ImGuiKey.J,
            VirtualKeyCode.VK_K => ImGuiKey.K,
            VirtualKeyCode.VK_L => ImGuiKey.L,
            VirtualKeyCode.VK_M => ImGuiKey.M,
            VirtualKeyCode.VK_N => ImGuiKey.N,
            VirtualKeyCode.VK_O => ImGuiKey.O,
            VirtualKeyCode.VK_P => ImGuiKey.P,
            VirtualKeyCode.VK_Q => ImGuiKey.Q,
            VirtualKeyCode.VK_R => ImGuiKey.R,
            VirtualKeyCode.VK_S => ImGuiKey.S,
            VirtualKeyCode.VK_T => ImGuiKey.T,
            VirtualKeyCode.VK_U => ImGuiKey.U,
            VirtualKeyCode.VK_V => ImGuiKey.V,
            VirtualKeyCode.VK_W => ImGuiKey.W,
            VirtualKeyCode.VK_X => ImGuiKey.X,
            VirtualKeyCode.VK_Y => ImGuiKey.Y,
            VirtualKeyCode.VK_Z => ImGuiKey.Z,
            VirtualKeyCode.VK_F1 => ImGuiKey.F1,
            VirtualKeyCode.VK_F2 => ImGuiKey.F2,
            VirtualKeyCode.VK_F3 => ImGuiKey.F3,
            VirtualKeyCode.VK_F4 => ImGuiKey.F4,
            VirtualKeyCode.VK_F5 => ImGuiKey.F5,
            VirtualKeyCode.VK_F6 => ImGuiKey.F6,
            VirtualKeyCode.VK_F7 => ImGuiKey.F7,
            VirtualKeyCode.VK_F8 => ImGuiKey.F8,
            VirtualKeyCode.VK_F9 => ImGuiKey.F9,
            VirtualKeyCode.VK_F10 => ImGuiKey.F10,
            VirtualKeyCode.VK_F11 => ImGuiKey.F11,
            VirtualKeyCode.VK_F12 => ImGuiKey.F12,
            VirtualKeyCode.VK_F13 => ImGuiKey.F13,
            VirtualKeyCode.VK_F14 => ImGuiKey.F14,
            VirtualKeyCode.VK_F15 => ImGuiKey.F15,
            VirtualKeyCode.VK_F16 => ImGuiKey.F16,
            VirtualKeyCode.VK_F17 => ImGuiKey.F17,
            VirtualKeyCode.VK_F18 => ImGuiKey.F18,
            VirtualKeyCode.VK_F19 => ImGuiKey.F19,
            VirtualKeyCode.VK_F20 => ImGuiKey.F20,
            VirtualKeyCode.VK_F21 => ImGuiKey.F21,
            VirtualKeyCode.VK_F22 => ImGuiKey.F22,
            VirtualKeyCode.VK_F23 => ImGuiKey.F23,
            VirtualKeyCode.VK_F24 => ImGuiKey.F24,
            VirtualKeyCode.VK_BROWSER_BACK => ImGuiKey.AppBack,
            VirtualKeyCode.VK_BROWSER_FORWARD => ImGuiKey.AppForward,
            _ => ImGuiKey.None,
        };
    }
}
