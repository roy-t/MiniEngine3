using System;
using System.Numerics;
using Windows.Win32.UI.KeyboardAndMouseInput;
namespace Mini.Engine.Windows
{
    public enum MouseButtons : ushort
    {
        Left = 0,
        Right = 1,
        Middle = 2,
        Four = 3,
        Five = 4
    }

    public sealed class Mouse
    {
        private enum ButtonState : ushort
        {
            JustPressed,
            Pressed,
            JustReleased,
            Released
        }

        private enum Direction : ushort
        {
            None,
            Up,
            Down
        }

        private readonly ButtonState[] Buttons;
        private Direction scrollDirection;

        public Mouse()
        {
            this.Buttons = new ButtonState[Enum.GetValues<MouseButtons>().Length];
            this.scrollDirection = Direction.None;
            this.Movement = Vector2.Zero;
        }

        public void Update()
        {
            this.scrollDirection = Direction.None;
            this.Movement = Vector2.Zero;

            for (var i = 0; i < this.Buttons.Length; i++)
            {
                this.Buttons[i] = this.Buttons[i] switch
                {
                    ButtonState.JustPressed => ButtonState.Pressed,
                    ButtonState.Pressed => ButtonState.Pressed,
                    ButtonState.JustReleased => ButtonState.Released,
                    ButtonState.Released => ButtonState.Released,
                    _ => throw new NotImplementedException(),
                };
            }
        }

        public Vector2 Movement { get; private set; }

        public bool ScrolledUp => this.scrollDirection == Direction.Up;
        public bool ScrolledDown => this.scrollDirection == Direction.Down;

        public bool Pressed(MouseButtons button)
            => this.Buttons[(int)button] == ButtonState.JustPressed;

        public bool Held(MouseButtons button)
            => this.Buttons[(int)button] == ButtonState.Pressed;

        public bool Released(MouseButtons button)
            => this.Buttons[(int)button] == ButtonState.JustReleased;

        internal void ProcessEvent(RAWINPUT input)
        {

        }
    }
}
