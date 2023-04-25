using System.Numerics;
using Windows.Win32.UI.Input;

namespace Mini.Engine.Windows;

public sealed class Keyboard : InputDevice
{
    public Keyboard() : base(256) { }

    public bool Pressed(ushort code)
    {
        return this.States[code].HasFlag(InputState.JustPressed);
    }

    public bool Held(ushort code)
    {
        return this.States[code].HasFlag(InputState.Pressed);
    }

    public bool Released(ushort code)
    {
        return this.States[code].HasFlag(InputState.JustReleased);
    }

    public float AsFloat(InputState state, ushort key)
    {
        return this.States[key].HasFlag(state) ? 1.0f : 0.0f;
    }

    public Vector2 AsVector(InputState state, ushort x, ushort y)
    {
        return new Vector2(this.AsFloat(state, x), this.AsFloat(state, y));
    }

    public Vector3 AsVector(InputState state, ushort x, ushort y, ushort z)
    {
        return new Vector3(this.AsFloat(state, x), this.AsFloat(state, y), this.AsFloat(state, z));
    }
    
    public Vector4 AsVector(InputState state, ushort x, ushort y, ushort z, ushort w)
    {
        return new Vector4(this.AsFloat(state, x), this.AsFloat(state, y), this.AsFloat(state, z), this.AsFloat(state, w));
    }

    internal override void NextFrame()
    {
        Decay(this.States);
    }

    internal override void NextEvent(RAWINPUT input)
    {
        var code = KeyboardDecoder.GetScanCode(input);
        var state = KeyboardDecoder.GetEvent(input);

        switch (state)
        {
            case KeyFlags.Make:
                this.States[code] = InputState.JustPressed | InputState.Pressed;
                break;
            case KeyFlags.Break:
                this.States[code] = InputState.JustReleased | InputState.Released;
                break;
        }
    }
}
