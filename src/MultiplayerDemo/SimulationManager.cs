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

    public static SimulationManager CreateSinglePlayer(string id)
    {
        var simulation = new Simulation();
        var controller = new SinglePlayerController(simulation);
        return new SimulationManager(id, simulation, controller);
    }

    public static SimulationManager CreateHost(MultiPlayerServer server, string id)
    {
        var simulation = new Simulation();
        var controller = new MultiPlayerHostController(server, simulation);
        return new SimulationManager(id, simulation, controller);
    }

    public static SimulationManager CreateClient(MultiPlayerServer server, string id)
    {
        var simulation = new Simulation();
        var controller = new MultiPlayerClientController(server, simulation);
        return new SimulationManager(id, simulation, controller);
    }

    private SimulationManager(string id, Simulation simulation, ISimulationController controller)
    {
        this.ResetEvent = new ManualResetEventSlim(false);
        this.Id = id;

        this.Simulation = simulation;
        this.Controller = controller;

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

            ImGui.TextColored(new Vector4(0.0f, 1.0f, 0.0f, 1.0f), $"SimTime {this.Controller.lastUpdateDurationMs:F2} ms");

            ImGui.TextColored(new Vector4(0.0f, 1.0f, 0.0f, 1.0f), $"State {this.Simulation.State}");

            var selected = 0;
            ImGui.ListBox("Log", ref selected, [.. this.Controller.Log.Reverse()], this.Controller.Log.Count);

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
