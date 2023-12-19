using Windows.Win32.UI.Input;

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
    /// The button is being held, not this does not generate new input events!
    /// </summary>    
    Held = 4,

    /// <summary>
    /// The button was just released
    /// </summary>
    Released = 8
}

public abstract class InputDevice
{
    protected readonly InputState[] States;

    internal int iterator;

    internal abstract void NextFrame();
    internal abstract void NextEvent(RAWINPUT input, bool hasFocus);

    internal InputDevice(int size)
    {
        this.States = new InputState[size];
    }

    internal static void Decay(InputState[] states)
    {
        for (var i = 0; i < states.Length; i++)
        {
            states[i] = states[i] switch
            {
                InputState.Pressed => InputState.Held,                
                InputState.Held => InputState.Held,
                InputState.Released => InputState.None,                
                InputState.None => InputState.None,
                _ => throw new NotImplementedException(),
            };
        }
    }
}
