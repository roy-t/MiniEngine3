namespace MultiplayerDemo;

public interface ISimulationController
{
    bool IsRunning { get; }
    string Name { get; }

    void Pause();
    void Start();
    void Update();
}