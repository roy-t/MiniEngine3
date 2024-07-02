using ImGuiNET;
using Mini.Engine.Windows;
using Mini.Engine.Windows.Events;
using static Windows.Win32.UI.Input.KeyboardAndMouse.VIRTUAL_KEY;

namespace Mini.Engine.UI;
// TODO: the input handler example from dearimgui is much more complex:
// https://github.com/ocornut/imgui/blob/master/backends/imgui_impl_win32.cpp
public sealed class ImGuiInputEventListener : IInputEventListener
{
    private static readonly int VK_SHIFT = 0x10;
    private static readonly int VK_CONTROL = 0x11;
    private static readonly int VK_ALT = 0x12;

    private readonly Win32Window Window;
    private ImGuiMouseCursor lastCursor;

    public ImGuiInputEventListener(Win32Window window)
    {
        this.Window = window;

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
        this.UpdateMousePosition();

        var mouseCursor = ImGui.GetIO().MouseDrawCursor ? ImGuiMouseCursor.None : ImGui.GetMouseCursor();
        if (mouseCursor != this.lastCursor)
        {
            this.lastCursor = mouseCursor;
            this.UpdateMouseCursor();
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

    private void UpdateMouseCursor()
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

    public void OnScroll(float delta)
    {
        var io = ImGui.GetIO();
        io.MouseWheel += delta;
    }

    public void OnHScroll(float delta)
    {
        var io = ImGui.GetIO();
        io.MouseWheelH += delta;
    }

    public void OnKeyDown(VirtualKeyCode key)
    {
        var io = ImGui.GetIO();
        io.KeysDown[key.Value] = true;

        if (key.Value == VK_ALT) { io.KeyAlt = true; }
        if (key.Value == VK_CONTROL) { io.KeyCtrl = true; }
        if (key.Value == VK_SHIFT) { io.KeyShift = true; }
    }

    public void OnKeyUp(VirtualKeyCode key)
    {
        var io = ImGui.GetIO();
        io.KeysDown[key.Value] = false;

        if (key.Value == VK_ALT) { io.KeyAlt = false; }
        if (key.Value == VK_CONTROL) { io.KeyCtrl = false; }
        if (key.Value == VK_SHIFT) { io.KeyShift = false; }
    }
}
