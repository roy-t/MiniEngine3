using System.Numerics;
using Windows.Win32.UI.Input;

namespace Mini.Engine.Windows;

public readonly record struct VirtualKeyCode(byte Value);

public sealed class Keyboard : InputDevice
{
    public Keyboard() : base(256) { }

    /// <summary>
    /// If the given button state changed to pressed this event
    /// </summary>
    public bool Pressed(ushort code)
    {
        return this.States[code] == InputState.Pressed;
    }

    /// <summary>
    /// If the given button was pressed both the previous and current event
    /// </summary>  
    public bool Held(ushort code)
    {
        return this.States[code] == InputState.Held;
    }

    /// <summary>
    /// If the given button state changed to released this event
    /// </summary>
    public bool Released(ushort code)
    {
        return this.States[code] == InputState.Released;
    }

    /// <summary>
    /// 1.0f if the button was an in the given state, 0.0f otherwise
    /// </summary>    
    public float AsFloat(InputState state, ushort key)
    {
        return this.States[key] == state ? 1.0f : 0.0f;
    }

    /// <summary>
    /// A vector with for each given button 1.0f if the button was an in the given state, 0.0f otherwise
    /// </summary>
    public Vector2 AsVector(InputState state, ushort x, ushort y)
    {
        return new Vector2(this.AsFloat(state, x), this.AsFloat(state, y));
    }

    /// <summary>
    /// A vector with for each given button 1.0f if the button was an in the given state, 0.0f otherwise
    /// </summary>
    public Vector3 AsVector(InputState state, ushort x, ushort y, ushort z)
    {
        return new Vector3(this.AsFloat(state, x), this.AsFloat(state, y), this.AsFloat(state, z));
    }

    /// <summary>
    /// A vector with for each given button 1.0f if the button was an in the given state, 0.0f otherwise
    /// </summary>
    public Vector4 AsVector(InputState state, ushort x, ushort y, ushort z, ushort w)
    {
        return new Vector4(this.AsFloat(state, x), this.AsFloat(state, y), this.AsFloat(state, z), this.AsFloat(state, w));
    }

    internal override void NextFrame()
    {
        Decay(this.States);
    }

    internal override void NextEvent(RAWINPUT input, bool hasFocus)
    {
        var code = KeyboardDecoder.GetScanCode(input);
        var state = KeyboardDecoder.GetEvent(input);

        switch (state)
        {
            case KeyFlags.Make:
                // To ignore repeated key inputs when a user holds a key
                // Only detect a key is pressed when we have focus
                if (hasFocus && this.States[code] != InputState.Held)
                {
                    this.States[code] = InputState.Pressed;
                }
                break;
            case KeyFlags.Break:
                // To ignore repeated key inputs when a user holds a key
                // Always detect when a key is released
                if (this.States[code] != InputState.None)
                {
                    this.States[code] = InputState.Released;
                }
                break;
        }
    }
}
