using System.Numerics;
using ImGuiNET;

namespace MultiplayerDemo;
public sealed class SimulationManager
{
    public Simulation Simulation { get; }
    public ISimulationController Controller { get; }

    private readonly ManualResetEventSlim ResetEvent;
    private readonly Thread ControlThread;
    private bool isRunning;

    public SimulationManager(string id)
    {
        this.ResetEvent = new ManualResetEventSlim(false);
        this.Id = id;

        this.Simulation = new Simulation();
        this.Controller = new SinglePlayerController(this.Simulation);

        this.ControlThread = new Thread(this.Loop)
        {
            IsBackground = true,
            Name = $"Controller - ${id}"
        };

        this.ControlThread.Start();
        this.isRunning = true;
    }

    public string Id { get; }

    public void Start()
    {
        this.Controller.Start();
        this.ResetEvent.Set();

    }

    public void Pause()
    {
        this.Controller.Pause();
        this.ResetEvent.Reset();
    }

    public void Stop()
    {
        this.Controller.Pause();
        this.isRunning = false;
        this.ResetEvent.Set();
        this.ControlThread.Join();
    }

    public void UpdateUserInterface()
    {
        if (ImGui.Begin($"{this.Id} - {this.Controller.Name}###{this.Id}"))
        {
            this.UpdateControls();

            ImGui.TextColored(new Vector4(0.0f, 1.0f, 0.0f, 1.0f), $"Step {this.Simulation.Step}");
            ImGui.SameLine();
            ImGui.TextColored(new Vector4(0.5f, 0.5f, 0.5f, 1.0f), $"Alpha {this.Simulation.Alpha}");
            ImGui.TextColored(new Vector4(0.0f, 1.0f, 0.0f, 1.0f), $"State {this.Simulation.State}");

            ImGui.End();
        }
    }

    private void UpdateControls()
    {
        if (this.isRunning)
        {
            var status = this.ResetEvent.IsSet ? "Running" : "Paused";
            ImGui.Text($"Status: {status}");
            if (this.ResetEvent.IsSet)
            {
                if (ImGui.Button("Pause"))
                {
                    this.Pause();
                }
            }
            else
            {
                if (ImGui.Button("Start"))
                {
                    this.Start();
                }
            }

            ImGui.SameLine();
            if (ImGui.Button("Stop"))
            {
                this.Stop();
            }
        }
        else
        {
            ImGui.Text("Stopped");
        }
    }

    private void Loop()
    {
        while (this.isRunning)
        {
            this.ResetEvent.Wait();
            this.Controller.Update();

            // TODO: but what about rendering, how to not busy loop, how to pause the controller, while rendering?
        }
    }
}
