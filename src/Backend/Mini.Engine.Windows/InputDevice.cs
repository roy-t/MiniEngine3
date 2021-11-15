using System;
using Windows.Win32.UI.KeyboardAndMouseInput;

namespace Mini.Engine.Windows;

[Flags]
public enum InputState : ushort
{
    Released = 2,
    JustPressed = 4,
    Pressed = 8,
    JustReleased = 16
}

public abstract class InputDevice
{
    protected readonly InputState[] States;

    internal int iterator;

    internal abstract void NextFrame();
    internal abstract void NextEvent(RAWINPUT input);

    internal InputDevice(int size)
    {
        this.States = new InputState[size];
        for (var i = 0; i < this.States.Length; i++)
        {
            this.States[i] = InputState.Released;
        }
    }

    internal static void Decay(InputState[] states)
    {
        for (var i = 0; i < states.Length; i++)
        {
            states[i] = states[i] switch
            {
                InputState.JustPressed => InputState.Pressed,
                InputState.JustPressed | InputState.Pressed => InputState.Pressed,
                InputState.Pressed => InputState.Pressed,
                InputState.JustReleased => InputState.Released,
                InputState.JustReleased | InputState.Released => InputState.Released,
                InputState.Released => InputState.Released,
                _ => throw new NotImplementedException(),
            };
        }
    }
}
