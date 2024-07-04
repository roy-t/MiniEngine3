using Mini.Engine.Windows.Events;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using static Windows.Win32.PInvoke;

namespace Mini.Engine.Windows;

public sealed class SimpleInputService : IInputEventListener
{
    private readonly Win32Window Window;

    public SimpleInputService(Win32Window window)
    {
        this.Window = window;
        this.Keyboard = new SimpleKeyboard();
        this.Mouse = new SimpleMouse();

        Win32Application.RegisterInputEventListener(window, this);
    }

    public SimpleKeyboard Keyboard { get; }
    public SimpleMouse Mouse { get; }

    public void OnButtonDown(MouseButton button)
    {
        this.Mouse.OnButtonDown(button);
    }

    public void OnButtonUp(MouseButton button)
    {
        this.Mouse.OnButtonUp(button);
    }

    public void OnScroll(int delta)
    {
        this.Mouse.OnScroll(delta);
    }

    public void OnHScroll(int delta)
    {
        this.Mouse.OnHScroll(delta);
    }

    public void OnChar(char character)
    {
        this.Keyboard.OnChar(character);
    }

    public void OnKeyDown(VirtualKeyCode key)
    {
        this.Keyboard.OnKeyDown(key);
    }

    public void OnKeyUp(VirtualKeyCode key)
    {
        this.Keyboard.OnKeyUp(key);
    }

    public void NextFrame()
    {
        this.Keyboard.NextFrame();
        this.Mouse.NextFrame();

        this.Mouse.UpdatePosition(this.Window.GetCursorPosition());
    }

    public static uint GetScanCode(VirtualKeyCode key)
    {
        return MapVirtualKey((uint)key, MAP_VIRTUAL_KEY_TYPE.MAPVK_VK_TO_VSC);
    }

    public static VirtualKeyCode GetVirtualKeyCode(ushort scanCode)
    {
        return (VirtualKeyCode)MapVirtualKey(scanCode, MAP_VIRTUAL_KEY_TYPE.MAPVK_VSC_TO_VK);
    }
}
