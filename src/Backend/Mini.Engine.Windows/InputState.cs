namespace Mini.Engine.Windows;

public enum InputState : byte
{
    /// <summary>
    /// The button is not being held and nothing happened to it recently
    /// </summary>
    None = 0,

    /// <summary>
    /// The button was just pressed
    /// </summary>
    Pressed = 2,

    /// <summary>
    /// The button is being held, note this does not generate new input events!
    /// </summary>    
    Held = 4,

    /// <summary>
    /// The button was just released
    /// </summary>
    Released = 8
}
