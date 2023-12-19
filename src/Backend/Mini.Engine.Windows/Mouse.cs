using System.Numerics;
using Windows.Win32.UI.Input;

namespace Mini.Engine.Windows;

public enum MouseButtons : ushort
{
    Left = 0,
    Right = 1,
    Middle = 2,
    Four = 3,
    Five = 4
}

public sealed class Mouse : InputDevice
{
    private enum Direction : ushort
    {
        None = 0,
        Up = 1,
        Down = 2
    }

    private Direction scrollDirection;

    public Mouse() : base(Enum.GetValues<MouseButtons>().Length) { }

    /// <summary>
    /// Relative movement per event, higher DPI mice send more events per inch moved
    /// </summary>
    public Vector2 Movement { get; private set; }

    /// <summary>
    /// If the mouse wheel was scrolled up in this event
    /// </summary>
    public bool ScrolledUp => this.scrollDirection == Direction.Up;

    /// <summary>
    /// If the mouse wheel was scrolled down in this event
    /// </summary>
    public bool ScrolledDown => this.scrollDirection == Direction.Down;

    /// <summary>
    /// If the given button state changed to pressed this event
    /// </summary>        
    public bool Pressed(MouseButtons button)
    {
        return this.States[(int)button] == InputState.Pressed;
    }

    /// <summary>
    /// If the given button was pressed both the previous and current event
    /// </summary>        
    public bool Held(MouseButtons button)
    {
        return this.States[(int)button] == InputState.Held;
    }

    /// <summary>
    /// If the given button state changed to released this event
    /// </summary>
    public bool Released(MouseButtons button)
    {
        return this.States[(int)button] == InputState.Released;
    }

    internal override void NextFrame()
    {
        this.scrollDirection = Direction.None;
        this.Movement = Vector2.Zero;        

        Decay(this.States);
    }

    internal override void NextEvent(RAWINPUT input, bool hasFocus)
    {
        this.Movement = MouseDecoder.GetPosition(input);
        var buttons = MouseDecoder.GetButtons(input);
                
        switch (buttons)
        {
            case ButtonFlags.LeftDown:
                if (hasFocus) { this.States[(int)MouseButtons.Left] = InputState.Pressed; }
                break;
            case ButtonFlags.LeftUp:
                this.States[(int)MouseButtons.Left] = InputState.Released;
                break;
            case ButtonFlags.RightDown:
                if (hasFocus) { this.States[(int)MouseButtons.Right] = InputState.Pressed; }
                break;
            case ButtonFlags.RightUp:
                this.States[(int)MouseButtons.Right] = InputState.Released;
                break;
            case ButtonFlags.MiddleDown:
                if (hasFocus) { this.States[(int)MouseButtons.Middle] = InputState.Pressed; }
                break;
            case ButtonFlags.MiddleUp:
                this.States[(int)MouseButtons.Middle] = InputState.Released;
                break;
            case ButtonFlags.Button4Down:
                if (hasFocus) { this.States[(int)MouseButtons.Four] = InputState.Pressed; }
                break;
            case ButtonFlags.Button4Up:
                this.States[(int)MouseButtons.Four] = InputState.Released;
                break;
            case ButtonFlags.Button5Down:
                if (hasFocus) { this.States[(int)MouseButtons.Five] = InputState.Pressed; }
                break;
            case ButtonFlags.Button5Up:
                this.States[(int)MouseButtons.Five] = InputState.Released;
                break;
            case ButtonFlags.MouseWheel:
                if (hasFocus)
                {
                    var scroll = MouseDecoder.GetMouseWheel(input);
                    if (scroll < 0)
                    {
                        this.scrollDirection = Direction.Down;
                    }
                    else if (scroll > 0)
                    {
                        this.scrollDirection = Direction.Up;
                    }
                    else
                    {
                        this.scrollDirection = Direction.None;
                    }
                }
                break;
        }
    }
}
