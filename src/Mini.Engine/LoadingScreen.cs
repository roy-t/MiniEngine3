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

[Service]
public sealed class LoadingScreen
{
    private const double ReportDelta = 1.0 / 60.0;

    private readonly ILogger Logger;
    private readonly Device Device;
    private readonly UICore UI;
    private readonly Services Services;

    public LoadingScreen(ILogger logger, Device device, UICore ui,  Services services)
    {
        this.Logger = logger.ForContext<LoadingScreen>();
        this.Device = device;
        this.UI = ui;
        this.Services = services;
    }

    public void Load(Type type)
    {
        var stopwatch = Stopwatch.StartNew();
                
        var reportWatch = Stopwatch.StartNew();        
        var accumulator = 0.0;

        var order = InjectableDependencies.CreateInitializationOrder(type);
        var index = 0;
        
        while (Win32Application.PumpMessages() && index < order.Count)
        {
            accumulator += reportWatch.Elapsed.TotalSeconds;
            reportWatch.Restart();

            var progress = index / (float)order.Count;
            
            if (accumulator >= ReportDelta)
            {
                var message = $"Loading {order[index].Name}";
                this.RenderWindow(message, progress);
                accumulator = 0.0;
            }

            this.Services.Resolve(order[index++]);
        }

        this.RenderWindow("Done", 1.0f);

        this.Logger.Information("Loading {@type} and {@count} dependencies took {@ms}ms ", type.FullName, order.Count, stopwatch.ElapsedMilliseconds);
    }

    private void RenderWindow(string message, float progress)
    {
        this.Device.ClearBackBuffer();
        this.UI.NewFrame((float)ReportDelta);

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
