using ImGuiNET;
using Mini.Engine.Windows;
using Mini.Engine.Windows.Events;
using static Windows.Win32.UI.Input.KeyboardAndMouse.VIRTUAL_KEY;

namespace Mini.Engine.UI;
public sealed class ImGuiInputEventListener : IInputEventListener
{
    private ImGuiMouseCursor lastCursor;

    public ImGuiInputEventListener()
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
    }

    public void OnKeyUp(VirtualKeyCode key)
    {
        var io = ImGui.GetIO();
        io.KeysDown[key.Value] = false;
    }
}
