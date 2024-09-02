namespace MultiplayerDemo;

public record MultiPlayerCommand(int ImAtStep);


public sealed class MultiPlayerHostController : ISimulationController
{
    private const double MinDeltaMs = 1.0 * 1000.0;
    private double MaxDeltaMs = 0.0;

    public bool IsRunning { get; }
    public string Name => nameof(MultiPlayerHostController);

    public void Pause()
    {
        throw new NotImplementedException();
    }

    public void Start()
    {
        throw new NotImplementedException();
    }

    public void Update()
    {
        throw new NotImplementedException();
    }
}
