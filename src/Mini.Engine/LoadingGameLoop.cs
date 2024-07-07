using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.UI;
using Serilog;

namespace Mini.Engine;

public sealed record LoadAction(string Description, Action Load);

[Service]
public sealed class LoadingGameLoop : IGameLoop
{
    private static readonly TimeSpan ReportDelta = TimeSpan.FromSeconds(1.0 / 60.0);

    private readonly ILogger Logger;
    private readonly Device Device;
    private readonly UICore UserInterface;
    private readonly Queue<LoadAction> Queue;

    private int total;

    public LoadingGameLoop(ILogger logger, Device device, UICore ui)
    {
        this.Logger = logger;
        this.Device = device;
        this.UserInterface = ui;
        this.Queue = new Queue<LoadAction>();
    }

    public void Add(LoadAction action)
    {
        this.Queue.Enqueue(action);
    }

    public void AddRange(IEnumerable<LoadAction> actions)
    {
        foreach (var action in actions)
        {
            this.Queue.Enqueue(action);
        }
    }


    public void Simulate() { }
    public void HandleInput(float elapsedRealWorldTime) { }

    public void Frame(float alpha, float elapsedRealWorldTime)
    {
        var message = "Initializing";

        // If total is zero this means we just entered the loading screen
        if (this.total == 0)
        {
            this.total = this.Queue.Count;
            this.Logger.Information("### Entered Loading Screen");
        }
        else
        {
            var total = Stopwatch.StartNew();
            do
            {
                var action = this.Queue.Dequeue();
                var single = Stopwatch.StartNew();
                action.Load();
                message = action.Description;
                this.Logger.Information("- {@description} took {@ms}ms", message, single.ElapsedMilliseconds);

            } while (this.Queue.Count > 0 && total.Elapsed < ReportDelta);
        }

        ImGui.SetNextWindowPos(new Vector2(0, Math.Max(0, this.Device.Height - 100)));
        ImGui.SetNextWindowSize(new Vector2(this.Device.Width, Math.Min(this.Device.Height, 100)));

        // TODO: when you remove all these flags you will notice that the window is akwardly placed.
        // Try to give it a fixed position that works for every screen and even when IMGui changes its layout rules
        if (ImGui.Begin(nameof(LoadingGameLoop),
           ImGuiWindowFlags.NoResize |
           ImGuiWindowFlags.NoMove |
           ImGuiWindowFlags.NoCollapse |
           ImGuiWindowFlags.NoSavedSettings |
           ImGuiWindowFlags.NoTitleBar |
           ImGuiWindowFlags.NoScrollbar |
           ImGuiWindowFlags.NoInputs |
           ImGuiWindowFlags.NoBackground |
           ImGuiWindowFlags.NoDecoration))
        {
            var progress = Math.Max(0.0f, (this.total - this.Queue.Count) / (float)this.total);
            var width = ImGui.GetWindowWidth();
            var size = new Vector2(0, 0);
            ImGui.ProgressBar(progress, size);
            ImGui.SameLine(0.0f, ImGui.GetStyle().ItemInnerSpacing.X);
            ImGui.TextUnformatted($"Loading: {message}");
            ImGui.End();
        }

        this.UserInterface.Render();

        if (this.Queue.Count == 0)
        {
            // Since the loading screen is kept in memory we need to reset total to zero so that we can properly detect
            // when we enter the loading screen again.
            this.total = 0;
            this.Logger.Information("### Completed Loading Screen");
        }
    }

    public void Resize(int width, int height)
    {
        this.UserInterface.Resize(width, height);
    }

    public void Dispose() { }
}
