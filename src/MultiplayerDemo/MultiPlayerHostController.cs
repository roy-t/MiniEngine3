﻿using System.Diagnostics;

namespace MultiplayerDemo;

public record MultiPlayerCommand(int ImAtStep);


public sealed class MultiPlayerHostController : ISimulationController
{
    private const int HostId = 0;
    private const double dt = 1.0 * 1000.0; // 1.0 / 60.0;
    private readonly Stopwatch Stopwatch;
    private double accumulator;

    private readonly MultiPlayerServer Server;
    private readonly Simulation Simulation;

    private readonly List<Message> Messages;
    private readonly List<string> LogList;

    public MultiPlayerHostController(MultiPlayerServer Server, Simulation simulation)
    {
        this.Messages = new List<Message>();
        this.Stopwatch = new Stopwatch();
        this.Server = Server;
        this.Simulation = simulation;
        this.LogList = new List<string>();
        this.Server.Host();
    }

    public bool IsRunning { get; private set; }
    public string Name => nameof(MultiPlayerHostController);
    public IReadOnlyList<string> Log => this.LogList;
    public double lastUpdateDurationMs { get; private set; }

    public void Pause()
    {
        this.Stopwatch.Reset();
        this.IsRunning = false;
    }

    public void Start()
    {
        this.Stopwatch.Restart();
        this.IsRunning = true;
    }

    public void Update()
    {
        this.Messages.AddRange(this.Server.ReceiveMessage(HostId));

        for (var i = this.Messages.Count - 1; i >= 0; i--)
        {
            var message = this.Messages[i];
            this.Messages.RemoveAt(i);

            this.LogList.Add($"{this.Simulation.Step}->{this.Simulation.Step + 1}: {message.Action}, {this.Simulation.State}->{this.Simulation.State + message.Action}");
            // TODO: verify action is still valid
            this.Simulation.Action(message.Action);

            // TODO: make target dt dynamic
            this.Server.BroadcastMessage(new Message(HostId, 0, this.Simulation.Step, dt, message.Action));

            // TODO: use step of client + delta of client to figure out how fast to update
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
                if (this.accumulator < dt)
                {
                    alpha = (float)(this.accumulator / dt);
                }
                this.Simulation.Forward(alpha);
            }
        }
    }
}
