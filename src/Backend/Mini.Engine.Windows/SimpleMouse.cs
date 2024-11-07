using System.Numerics;

namespace Mini.Engine.Windows;

public sealed class SimpleMouse : SimpleInputDevice
{
    private const int WHEEL_DELTA = 120;

    private int scrollState;
    private int nextScrollState;
    private int hScrollState;
    private int nextHScrollState;
    private Vector2 position;
    private Vector2 nextPostion;
    private Vector2 movement;

    internal SimpleMouse() : base(Enum.GetValues<MouseButton>().Length) { }

    /// <summary>
    /// Relative movement per event, higher DPI mice send more events per inch moved
    /// </summary>
    public Vector2 Movement => this.movement;

    /// <summary>
    /// The position of the mouse cursor relative to the upper left corner of the main window, in pixels
    /// </summary>
    public Vector2 Position => this.position;

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

        this.movement = this.nextPostion - this.position;
        this.position = this.nextPostion;

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

    internal void OnHScroll(int delta)
    {
        this.nextHScrollState += delta;
    }

    internal void OnScroll(int delta)
    {
        this.nextScrollState += delta;
    }

    internal void UpdatePosition(Vector2 position)
    {
        this.nextPostion = position;
    }
}
