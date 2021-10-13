using System;
using System.Numerics;
using Vortice.DirectInput;

namespace Mini.Engine.Input
{
    public sealed class MouseController : IDisposable
    {
        private readonly IDirectInput8 Instance;
        private readonly IDirectInputDevice8 Mouse;

        private MouseState LastState;
        private MouseState CurrentState;

        public MouseController(IntPtr hwnd)
        {
            this.Instance = DInput.DirectInput8Create();
            this.Mouse = this.Instance.CreateDevice(PredefinedDevice.SysMouse);

            this.Mouse.SetDataFormat<RawMouseState>();
            this.Mouse.SetCooperativeLevel(hwnd, CooperativeLevel.Background | CooperativeLevel.NonExclusive);
            this.Mouse.Acquire();

            this.LastState = new MouseState();
            this.CurrentState = new MouseState();
        }

        public void Update()
        {
            this.LastState = this.CurrentState;
            this.CurrentState = this.Mouse.GetCurrentMouseState();
        }

        public bool Pressed(int button)
        {
            return this.Is(button, InputState.JustPressed);
        }

        public bool Held(int button)
        {
            return this.Is(button, InputState.Pressed);
        }

        public bool Released(int button)
        {
            return this.Is(button, InputState.JustReleased);
        }

        public Vector2 Movement => new(this.CurrentState.X, this.CurrentState.Y);

        public bool ScrolledUp()
        {
            return this.CurrentState.Z > 0;
        }

        public bool ScrolledDown()
        {
            return this.CurrentState.Z < 0;
        }

        private bool Is(int button, InputState state)
        {
            return state switch
            {
                InputState.JustPressed => !this.LastState.Buttons[button] && this.CurrentState.Buttons[button],
                InputState.Pressed => this.LastState.Buttons[button] && this.CurrentState.Buttons[button],
                InputState.JustReleased => this.LastState.Buttons[button] && !this.CurrentState.Buttons[button],
                InputState.Released => !this.LastState.Buttons[button] && !this.CurrentState.Buttons[button],
                _ => throw new ArgumentOutOfRangeException(nameof(state)),
            };
        }

        public void Dispose()
        {
            this.Mouse.Dispose();
            this.Instance.Dispose();
        }
    }
}
