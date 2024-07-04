namespace Mini.Engine.Windows.Events;

public interface IInputEventListener
{
    /// <summary>
    /// The button was pressed
    /// </summary>
    void OnButtonDown(MouseButton button);

    /// <summary>
    /// The button was released
    /// </summary>
    void OnButtonUp(MouseButton button);

    /// <summary>
    /// The amount the scroll wheel scrolled vertically in this event. In most cases this value will have a 
    /// magnitude of 120 per event. Which Microsoft defines as one 'increment' on the mouse wheel. Some special
    /// mice allow for 'smooth' scrolling and will send more events, with a smaller magnitude.
    /// 
    /// A positive value indicates that the wheel was rotated forward, away from the user;
    /// a negative value indicates that the wheel was rotated backward, towards the user.
    /// </summary>
    /// <param name="delta"></param>
    void OnScroll(int delta);

    /// <summary>
    /// The amount the scroll wheel scrolled horizontally in this event. In most cases this value will have a 
    /// magnitude of 120 per event. Which Microsoft defines as one 'increment' on the mouse wheel. Some special
    /// mice allow for 'smooth' scrolling and will send more events, with a smaller magnitude.
    /// 
    /// A positive value indicates that the wheel was rotated to the right;
    /// a negative value indicates that the wheel was rotated to the left.
    /// </summary>
    /// <param name="delta"></param>
    void OnHScroll(int delta);

    /// <summary>
    /// A key (-combination) was pressed that can be translated to a unicode character
    /// </summary>
    void OnChar(char character);

    /// <summary>
    /// A key was pressed, note this potentially also generates a call to OnChar.
    /// </summary>
    void OnKeyDown(VirtualKeyCode key);

    /// <summary>
    /// A key was released
    /// </summary>
    void OnKeyUp(VirtualKeyCode key);
}

