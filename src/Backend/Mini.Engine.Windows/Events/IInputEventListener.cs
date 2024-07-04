namespace Mini.Engine.Windows.Events;

public interface IInputEventListener
{
    void OnButtonDown(MouseButton button);
    void OnButtonUp(MouseButton button);
    void OnScroll(float delta);
    void OnHScroll(float delta);
    void OnChar(char character);
    void OnKeyDown(VirtualKeyCode key);
    void OnKeyUp(VirtualKeyCode key);
}

