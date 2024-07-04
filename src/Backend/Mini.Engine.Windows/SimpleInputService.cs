using System.Numerics;
using Mini.Engine.Windows.Events;

namespace Mini.Engine.Windows;

public abstract class SimpleInputDevice
{
    protected readonly InputState[] State;
    protected readonly InputState[] NextState;
    public SimpleInputDevice(int states)
    {
        this.State = new InputState[states];
        this.NextState = new InputState[states];
    }

    public virtual void NextFrame()
    {
        for (var i = 0; i < this.State.Length; i++)
        {
            this.State[i] = this.NextState[i];
            this.NextState[i] = this.NextState[i] switch
            {
                InputState.Pressed => InputState.Held,
                InputState.Held => InputState.Held,
                InputState.Released => InputState.None,
                InputState.None => InputState.None,
                _ => throw new ArgumentOutOfRangeException(nameof(InputState)),
            };
        }
    }
}

public sealed class SimpleMouse : SimpleInputDevice
{
    private const int WHEEL_DELTA = 120;

    private int scrollState;
    private int nextScrollState;
    private int hScrollState;
    private int nextHScrollState;
    private Vector2 movement;
    private Vector2 nextMovement;

    public SimpleMouse() : base(Enum.GetValues<MouseButton>().Length) { }

    /// <summary>
    /// Relative movement per event, higher DPI mice send more events per inch moved
    /// </summary>
    public Vector2 Movement => this.Movement;

    /// <summary>
    /// The increments the scroll wheel scrolled vertically in this frame.
    /// A positive value indicates that the wheel was rotated forward, away from the user;
    /// a negative value indicates that the wheel was rotated backward, towards the user.
    /// </summary>
    public int ScrollDelta => this.scrollState / WHEEL_DELTA;

    /// <summary>
    /// The delta the scroll wheel scrolled horizontally this frame.
    /// A positive value indicates that the wheel was rotated to the right;
    /// a negative value indicates that the wheel was rotated to the left.
    /// </summary>
    public int HorizontalScrollDelta => this.hScrollState / WHEEL_DELTA;

    /// <summary>
    /// If the given button state changed to pressed this frame
    /// </summary>        
    public bool Pressed(MouseButton button)
    {
        return this.State[(int)button] == InputState.Pressed;
    }

    /// <summary>
    /// If the given button was pressed both the previous and current frame
    /// </summary>        
    public bool Held(MouseButton button)
    {
        return this.State[(int)button] == InputState.Held;
    }

    /// <summary>
    /// If the given button state changed to released this frame
    /// </summary>
    public bool Released(MouseButton button)
    {
        return this.State[(int)button] == InputState.Released;
    }

    public override void NextFrame()
    {
        this.scrollState = this.nextScrollState;
        this.nextScrollState = 0;

        this.hScrollState = this.nextHScrollState;
        this.nextHScrollState = 0;

        this.movement = this.nextMovement;
        this.nextMovement = Vector2.Zero;

        base.NextFrame();
    }

    internal void OnButtonDown(MouseButton button)
    {
        this.NextState[(int)button] = InputState.Pressed;
    }

    internal void OnButtonUp(MouseButton button)
    {
        this.NextState[(int)button] = InputState.Released;
    }

    internal void OnHScroll(float delta)
    {
        throw new NotImplementedException();
    }

    internal void OnScroll(float delta)
    {
        throw new NotImplementedException();
    }
}
public sealed class SimpleKeyboard : SimpleInputDevice
{
    private string typed;
    private string nextTyped;

    public SimpleKeyboard() : base(VirtualKeyCode.MaxValue + 1)
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


public sealed class SimpleInputService : IInputEventListener
{
    public SimpleInputService()
    {
        this.Keyboard = new SimpleKeyboard();
        this.Mouse = new SimpleMouse();
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
}
