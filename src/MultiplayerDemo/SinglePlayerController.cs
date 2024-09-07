using System.Diagnostics;

namespace MultiplayerDemo;
public sealed class SinglePlayerController : ISimulationController
{
    private const double dt = 1.0 * 1000.0; // 1.0 / 60.0;
    private readonly Stopwatch Stopwatch;
    private double accumulator;
    private readonly Simulation Simulation;
    private readonly List<string> LogList;

    public SinglePlayerController(Simulation simulation)
    {
        this.Simulation = simulation;
        this.Stopwatch = new Stopwatch();
        this.LogList = new List<string>();
    }

    public double lastUpdateDurationMs { get; private set; }
    public bool IsRunning { get; private set; }
    public string Name => nameof(SinglePlayerController);
    public IReadOnlyList<string> Log => this.LogList;

    public void Start()
    {
        this.Stopwatch.Restart();
        this.IsRunning = true;
    }

    public void Pause()
    {
        this.Stopwatch.Reset();
        this.IsRunning = false;
    }

    public void Update()
    {
        if (this.IsRunning)
        {
            this.lastUpdateDurationMs = this.Stopwatch.Elapsed.TotalMilliseconds;
            this.accumulator += this.lastUpdateDurationMs;

            this.accumulator = Math.Clamp(this.accumulator, 0.0, dt * 10.0);
            this.Stopwatch.Restart();

            while (this.accumulator >= dt)
            {
                this.accumulator -= dt;
                var alpha = 0.0f;
                if (this.accumulator < dt)
                {
                    alpha = (float)(this.accumulator / dt);
                }
                this.Tick(alpha);
            }

            Thread.Sleep(1); // Simulate expensive computations
        }
    }

    private void Tick(float alpha)
    {
        // Simulate some inputs
        var c = Random.Shared.Next(10);
        if (c < 3)
        {
            this.LogList.Add($"Step:{this.Simulation.Step}->{this.Simulation.Step + 1}, Action: {c}, State: {this.Simulation.State}->{this.Simulation.State + c}");
            this.Simulation.Action(c);
        }
        this.Simulation.Forward(alpha);
    }
}
