using System.Diagnostics;
using Mini.Engine.Configuration;
using Mini.Engine.Debugging;
using Mini.Engine.Diesel.Tracks;
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
    CascadedShadowMapSystem CascadedShadowMap,
    TrackManager TrackManager
);

[Service]
internal sealed class UpdatePipeline
{
    private readonly MetricService MetricService;
    private readonly UpdateSystems Systems;    
    private readonly Stopwatch Stopwatch;

    private readonly Queue<Task> WorkQueue;

    public UpdatePipeline(MetricService metricService, UpdateSystems systems)
    {
        this.MetricService = metricService;
        this.Systems = systems;
        this.WorkQueue = new Queue<Task>();
        this.Stopwatch = new Stopwatch();
    }

    public void Run()
    {
        this.Stopwatch.Restart();

        // The following systems depend directly on each other, so they cannot run in parallel
        this.Systems.ComponentLifeCycle.Run();        
        this.Systems.Transform.Run();
        this.Systems.Camera.Update();
        this.Systems.CascadedShadowMap.Update();

        this.Systems.TrackManager.Update();

        // TODO: add more systems that can run in parallel
        //this.ProcessQueue();

        this.MetricService.Update("UpdatePipeline.Run.Millis", (float)this.Stopwatch.Elapsed.TotalMilliseconds);
    }

    //private void ProcessQueue()
    //{
    //    while (this.WorkQueue.Any())
    //    {
    //        var task = this.WorkQueue.Dequeue();
    //        task.Wait();            
    //    }
    //}

    //private void Enqueue(Task task)
    //{
    //    this.WorkQueue.Enqueue(task);
    //}
}
