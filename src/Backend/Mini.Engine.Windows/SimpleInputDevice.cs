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
