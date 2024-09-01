using System.Diagnostics;

namespace MultiplayerDemo;
public sealed class SinglePlayerController : ISimulationController
{
    const double dt = 1.0 * 1000.0; // 1.0 / 60.0;
    private readonly Stopwatch Stopwatch;
    private double accumulator;
    private readonly Simulation Simulation;

    public SinglePlayerController(Simulation simulation)
    {
        this.Simulation = simulation;
        this.Stopwatch = new Stopwatch();
    }

    public bool IsRunning { get; private set; }
    public string Name => nameof(SinglePlayerController);

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
            this.accumulator += this.Stopwatch.Elapsed.TotalMilliseconds;

            // TODO: signal is running slowly?
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
        }
    }

    private void Tick(float alpha)
    {
        this.Simulation.Forward(alpha);
    }
}
