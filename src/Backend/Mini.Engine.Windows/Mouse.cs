using System;
using System.Numerics;
using Windows.Win32.UI.KeyboardAndMouseInput;
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

    public Vector2 Movement { get; private set; }

    public bool ScrolledUp => this.scrollDirection == Direction.Up;
    public bool ScrolledDown => this.scrollDirection == Direction.Down;

    public bool Pressed(MouseButtons button)
    {
        return this.States[(int)button] == InputState.JustPressed;
    }

    public bool Held(MouseButtons button)
    {
        return this.States[(int)button] == InputState.Pressed;
    }

    public bool Released(MouseButtons button)
    {
        return this.States[(int)button] == InputState.JustReleased;
    }

    internal override void NextFrame()
    {
        this.scrollDirection = Direction.None;
        this.Movement = Vector2.Zero;

        Decay(this.States);
    }

    internal override void NextEvent(RAWINPUT input)
    {
        var position = MouseDecoder.GetPosition(input);
        this.Movement = position;

        var buttons = MouseDecoder.GetButtons(input);
        switch (buttons)
        {
            case ButtonFlags.LeftDown:
                this.States[(int)MouseButtons.Left] = InputState.JustPressed;
                break;
            case ButtonFlags.LeftUp:
                this.States[(int)MouseButtons.Left] = InputState.JustReleased;
                break;
            case ButtonFlags.RightDown:
                this.States[(int)MouseButtons.Right] = InputState.JustPressed;
                break;
            case ButtonFlags.RightUp:
                this.States[(int)MouseButtons.Right] = InputState.JustReleased;
                break;
            case ButtonFlags.MiddleDown:
                this.States[(int)MouseButtons.Middle] = InputState.JustPressed;
                break;
            case ButtonFlags.MiddleUp:
                this.States[(int)MouseButtons.Middle] = InputState.JustReleased;
                break;
            case ButtonFlags.Button4Down:
                this.States[(int)MouseButtons.Four] = InputState.JustPressed;
                break;
            case ButtonFlags.Button4Up:
                this.States[(int)MouseButtons.Four] = InputState.JustReleased;
                break;
            case ButtonFlags.Button5Down:
                this.States[(int)MouseButtons.Five] = InputState.JustPressed;
                break;
            case ButtonFlags.Button5Up:
                this.States[(int)MouseButtons.Five] = InputState.JustReleased;
                break;
            case ButtonFlags.MouseWheel:
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
                break;
        }
    }
}
