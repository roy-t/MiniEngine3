namespace MultiplayerDemo;

public sealed class Simulation()
{
    public int Step { get; private set; }
    public float Alpha { get; private set; }

    public void Forward(float alpha)
    {
        this.Alpha = alpha;
        this.Step++;
    }
}
