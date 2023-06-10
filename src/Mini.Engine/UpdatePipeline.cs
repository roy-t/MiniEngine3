using System.Diagnostics;
using Mini.Engine.Configuration;
using Mini.Engine.Debugging;
using Mini.Engine.ECS.Components;
using Mini.Engine.Graphics.Cameras;
using Mini.Engine.Graphics.Lighting.ShadowingLights;
using Mini.Engine.Graphics.Transforms;

namespace Mini.Engine;

[Service]
internal sealed record class UpdateSystems
(
    CameraSystem Camera,
    ComponentLifeCycleSystem ComponentLifeCycle,
    TransformSystem Transform,
    CascadedShadowMapSystem CascadedShadowMap
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

        this.Systems.ComponentLifeCycle.Run();
        this.Systems.Transform.Run();
        this.Systems.Camera.Update();
        this.Systems.CascadedShadowMap.Update();

        this.MetricService.Update("UpdatePipeline.Run.Millis", (float)this.Stopwatch.Elapsed.TotalMilliseconds);
    }
}
