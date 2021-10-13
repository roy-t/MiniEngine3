using System;
using System.Numerics;
using Vortice.DirectInput;

namespace Mini.Engine.Input
{
    public sealed class KeyboardController : IDisposable
    {
        private readonly IDirectInput8 Instance;
        private readonly IDirectInputDevice8 Keyboard;

        private KeyboardState LastState;
        private KeyboardState CurrentState;

        public KeyboardController(IntPtr hwnd)
        {
            this.Instance = DInput.DirectInput8Create();
            this.Keyboard = this.Instance.CreateDevice(PredefinedDevice.SysKeyboard);

            this.Keyboard.SetDataFormat<RawKeyboardState>();
            this.Keyboard.SetCooperativeLevel(hwnd, CooperativeLevel.Background | CooperativeLevel.NonExclusive);
            this.Keyboard.Acquire();

            this.LastState = new KeyboardState();
            this.CurrentState = new KeyboardState();
        }

        public void Update()
        {
            this.LastState = this.CurrentState;
            this.CurrentState = this.Keyboard.GetCurrentKeyboardState();
        }

        public bool Pressed(Key key)
        {
            return this.Is(key, InputState.JustPressed);
        }

        public bool Held(Key key)
        {
            return this.Is(key, InputState.Pressed);
        }

        public bool Released(Key key)
        {
            return this.Is(key, InputState.JustReleased);
        }

        public float AsFloat(InputState state, Key key)
        {
            return this.Is(key, state) ? 1.0f : 0.0f;
        }

        public Vector2 AsVector(InputState state, Key x, Key y)
        {
            return new Vector2(this.AsFloat(state, x), this.AsFloat(state, y));
        }

        public Vector3 AsVector(InputState state, Key x, Key y, Key z)
        {
            return new Vector3(this.AsFloat(state, x), this.AsFloat(state, y), this.AsFloat(state, z));
        }

        public Vector4 AsVector(InputState state, Key x, Key y, Key z, Key w)
        {
            return new Vector4(this.AsFloat(state, x), this.AsFloat(state, y), this.AsFloat(state, z), this.AsFloat(state, w));
        }

        private bool Is(Key key, InputState state)
        {
            return state switch
            {
                InputState.JustPressed => !this.LastState.IsPressed(key) && this.CurrentState.IsPressed(key),
                InputState.Pressed => this.LastState.IsPressed(key) && this.CurrentState.IsPressed(key),
                InputState.JustReleased => this.LastState.IsPressed(key) && !this.CurrentState.IsPressed(key),
                InputState.Released => !this.LastState.IsPressed(key) && !this.CurrentState.IsPressed(key),
                _ => throw new ArgumentOutOfRangeException(nameof(state)),
            };
        }

        public void Dispose()
        {
            this.Keyboard.Dispose();
            this.Instance.Dispose();
        }
    }
}