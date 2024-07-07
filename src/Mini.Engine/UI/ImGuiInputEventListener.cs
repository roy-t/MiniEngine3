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

        var io = ImGui.GetIO();

        io.KeyMap[(int)ImGuiKey.Tab] = (int)VirtualKeyCode.VK_TAB;
        io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)VirtualKeyCode.VK_LEFT;
        io.KeyMap[(int)ImGuiKey.RightArrow] = (int)VirtualKeyCode.VK_RIGHT;
        io.KeyMap[(int)ImGuiKey.UpArrow] = (int)VirtualKeyCode.VK_UP;
        io.KeyMap[(int)ImGuiKey.DownArrow] = (int)VirtualKeyCode.VK_DOWN;
        io.KeyMap[(int)ImGuiKey.PageUp] = (int)VirtualKeyCode.VK_PRIOR;
        io.KeyMap[(int)ImGuiKey.PageDown] = (int)VirtualKeyCode.VK_NEXT;
        io.KeyMap[(int)ImGuiKey.Home] = (int)VirtualKeyCode.VK_HOME;
        io.KeyMap[(int)ImGuiKey.End] = (int)VirtualKeyCode.VK_END;
        io.KeyMap[(int)ImGuiKey.Insert] = (int)VirtualKeyCode.VK_INSERT;
        io.KeyMap[(int)ImGuiKey.Delete] = (int)VirtualKeyCode.VK_DELETE;
        io.KeyMap[(int)ImGuiKey.Backspace] = (int)VirtualKeyCode.VK_BACK;
        io.KeyMap[(int)ImGuiKey.Space] = (int)VirtualKeyCode.VK_SPACE;
        io.KeyMap[(int)ImGuiKey.Enter] = (int)VirtualKeyCode.VK_RETURN;
        io.KeyMap[(int)ImGuiKey.Escape] = (int)VirtualKeyCode.VK_ESCAPE;
        io.KeyMap[(int)ImGuiKey.KeypadEnter] = (int)VirtualKeyCode.VK_RETURN;
        io.KeyMap[(int)ImGuiKey.A] = 'A';
        io.KeyMap[(int)ImGuiKey.C] = 'C';
        io.KeyMap[(int)ImGuiKey.V] = 'V';
        io.KeyMap[(int)ImGuiKey.X] = 'X';
        io.KeyMap[(int)ImGuiKey.Y] = 'Y';
        io.KeyMap[(int)ImGuiKey.Z] = 'Z';
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
        io.KeysDown[(int)key] = true;

        if (key == VirtualKeyCode.VK_MENU) { io.KeyAlt = true; }
        if (key == VirtualKeyCode.VK_CONTROL) { io.KeyCtrl = true; }
        if (key == VirtualKeyCode.VK_SHIFT) { io.KeyShift = true; }
    }

    public void OnKeyUp(VirtualKeyCode key)
    {
        var io = ImGui.GetIO();
        io.KeysDown[(int)key] = false;

        if (key == VirtualKeyCode.VK_MENU) { io.KeyAlt = false; }
        if (key == VirtualKeyCode.VK_CONTROL) { io.KeyCtrl = false; }
        if (key == VirtualKeyCode.VK_SHIFT) { io.KeyShift = false; }
    }
}
