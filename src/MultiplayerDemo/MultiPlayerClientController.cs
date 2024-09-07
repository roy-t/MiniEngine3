using System.Diagnostics;

namespace MultiplayerDemo;
internal class MultiPlayerClientController : ISimulationController
{
    private int ClientId = 0;

    private const double dt = 1.0 * 1000.0; // 1.0 / 60.0;
    private readonly Stopwatch Stopwatch;
    private double accumulator;

    private readonly MultiPlayerServer Server;
    private readonly Simulation Simulation;

    private readonly List<Message> Messages;
    private readonly List<string> LogList;

    public MultiPlayerClientController(MultiPlayerServer Server, Simulation simulation)
    {
        this.Messages = new List<Message>();
        this.Stopwatch = new Stopwatch();
        this.Server = Server;
        this.Simulation = simulation;
        this.ClientId = this.Server.Connect();
        this.LogList = new List<string>();
    }

    public bool IsRunning { get; private set; }
    public string Name => nameof(MultiPlayerHostController);
    public IReadOnlyList<string> Log => this.LogList;

    public double lastUpdateDurationMs { get; private set; }

    public void Pause()
    {
        // TODO: should send pause message to the host
    }

    public void Start()
    {
        this.Stopwatch.Restart();
        this.IsRunning = true;
    }

    public void Update()
    {
        this.Messages.AddRange(this.Server.ReceiveMessage(this.ClientId));

        for (var i = this.Messages.Count - 1; i >= 0; i--)
        {
            var message = this.Messages[i];
            if (message.Step > this.Simulation.Step)
            {
                // Wait
            }
            else if (message.Step == this.Simulation.Step)
            {
                // Process
                this.Messages.RemoveAt(i);
                this.LogList.Add($"{this.Simulation.Step}->{this.Simulation.Step + 1}: {message.Action}, {this.Simulation.State}->{this.Simulation.State + message.Action}");
                this.Simulation.Action(message.Action);

            }
            else // message.Step < this.Simulation.Step
            {
                throw new Exception("Failed to incorporate old message");
            }
        }

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
                if (this.accumulator < dt) // TODO: vary DT based on server update speed
                {
                    alpha = (float)(this.accumulator / dt);
                }
                this.Tick(alpha);
            }
        }
    }

    private void Tick(float alpha)
    {
        // Simulate some inputs
        var c = Random.Shared.Next(10);
        if (c < 3)
        {
            this.Server.SendMessage(new Message(this.ClientId, 0, this.Simulation.Step, this.lastUpdateDurationMs, c));
        }
        this.Simulation.Forward(alpha);
    }
}
