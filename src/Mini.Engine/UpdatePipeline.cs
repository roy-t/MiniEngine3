using System.Diagnostics;
using Mini.Engine.Configuration;
using Mini.Engine.Debugging;
using Mini.Engine.Diesel.Tracks;
using Mini.Engine.ECS.Components;
using Mini.Engine.Graphics.Transforms;

namespace Mini.Engine;

[Service]
internal sealed record class UpdateSystems
(
    ComponentLifeCycleSystem ComponentLifeCycle,
    TransformSystem Transform,
    TrackManager TrackManager
);

[Service]
internal sealed class UpdatePipeline
{
    private readonly MetricService MetricService;
    private readonly UpdateSystems Systems;
    private readonly Stopwatch Stopwatch;

    public UpdatePipeline(MetricService metricService, UpdateSystems systems)
    {
        this.MetricService = metricService;
        this.Systems = systems;
        this.Stopwatch = new Stopwatch();
    }

    public void Run()
    {
        this.Stopwatch.Restart();

        // The following systems depend directly on each other, so they cannot run in parallel
        this.Systems.ComponentLifeCycle.Run();
        this.Systems.Transform.Run();

        this.MetricService.Update("UpdatePipeline.Run.Millis", (float)this.Stopwatch.Elapsed.TotalMilliseconds);
    }
}
