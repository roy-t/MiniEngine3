using System;
using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.UI;
using Mini.Engine.Windows;
using Serilog;

namespace Mini.Engine;

public sealed record LoadAction(string Description, Action Load);

[Service]
public sealed class LoadingScreen
{
    private static TimeSpan ReportDelta = TimeSpan.FromSeconds(1.0 / 60.0);

    private readonly ILogger Logger;
    private readonly Device Device;
    private readonly UICore UI;
    private readonly Services Services;

    public LoadingScreen(ILogger logger, Device device, UICore ui, Services services)
    {
        this.Logger = logger.ForContext<LoadingScreen>();
        this.Device = device;
        this.UI = ui;
        this.Services = services;
    }

    public void Load(IReadOnlyList<LoadAction> actions, string description)
    {
        this.Logger.Information("### Loading {@description}", description);

        var stopwatch = Stopwatch.StartNew();
        var totalTime = TimeSpan.Zero;
        var accumulator = TimeSpan.MaxValue;
        for (var i = 0; i < actions.Count; i++)
        {
            if (accumulator >= ReportDelta)
            {
                var progress = i / (float)actions.Count;
                var message = $"Loading {actions[i].Description}";
                this.RenderWindow(message, progress);
                accumulator = TimeSpan.Zero;
            }

            stopwatch.Restart();
            actions[i].Load();
            var elapsed = stopwatch.Elapsed;
            this.Logger.Information("- Action {@index}/{@count}: {@description} took {@ms}ms",i + 1, actions.Count, actions[i].Description, elapsed.TotalMilliseconds);
            totalTime += elapsed;
            accumulator += elapsed;
        }

        this.RenderWindow("Completed", 1.0f);

        this.Logger.Information("### Loading {@description} took {@ms}ms", description, actions.Count, totalTime.TotalMilliseconds);        
    }

    private void RenderWindow(string message, float progress)
    {
        this.Device.ImmediateContext.ClearBackBuffer();

        this.UI.NewFrame((float)ReportDelta.TotalMilliseconds);

        ImGui.SetNextWindowPos(new Vector2(0, Math.Max(0, this.Device.Height - 100)));
        ImGui.SetNextWindowSize(new Vector2(this.Device.Width, Math.Min(this.Device.Height, 100)));

        if (ImGui.Begin(nameof(LoadingScreen),
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
            ImGui.ProgressBar(progress);
            ImGui.Text(message);
            ImGui.End();
        }

        this.UI.Render();
        this.Device.Present();
    }
}
