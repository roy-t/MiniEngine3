namespace MultiplayerDemo;

public interface ISimulationController
{
    bool IsRunning { get; }
    string Name { get; }

    double lastUpdateDurationMs { get; }

    void Pause();
    void Start();
    void Update();

    IReadOnlyList<string> Log { get; }
}