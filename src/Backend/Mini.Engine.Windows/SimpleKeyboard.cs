using System.Numerics;

namespace Mini.Engine.Windows;

public sealed class SimpleKeyboard : SimpleInputDevice
{
    private string typed;
    private string nextTyped;

    public SimpleKeyboard() : base(Enum.GetValues<VirtualKeyCode>().Length)
    {
        this.typed = string.Empty;
        this.nextTyped = string.Empty;
    }

    /// <summary>
    /// If the given button state changed to pressed this frame
    /// </summary>
    public bool Pressed(ushort code)
    {
        return this.State[code] == InputState.Pressed;
    }

    /// <summary>
    /// If the given button was pressed both the previous and current frame
    /// </summary>  
    public bool Held(ushort code)
    {
        return this.State[code] == InputState.Held;
    }

    /// <summary>
    /// If the given button state changed to released this frame
    /// </summary>
    public bool Released(ushort code)
    {
        return this.State[code] == InputState.Released;
    }

    /// <summary>
    /// 1.0f if the button was an in the given state, 0.0f otherwise
    /// </summary>    
    public float AsFloat(InputState state, ushort key)
    {
        return this.State[key] == state ? 1.0f : 0.0f;
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

    internal void OnChar(char character)
    {
        this.nextTyped += character;
    }

    internal void OnKeyDown(VirtualKeyCode key)
    {
        this.NextState[key.Value] = InputState.Pressed;
    }

    internal void OnKeyUp(VirtualKeyCode key)
    {
        this.NextState[key.Value] = InputState.Released;
    }

    public override void NextFrame()
    {
        this.typed = this.nextTyped;
        this.nextTyped = string.Empty;

        base.NextFrame();
    }
}
