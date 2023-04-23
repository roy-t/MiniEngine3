using System.Diagnostics;
using Mini.Engine.Configuration;
using Mini.Engine.Debugging;
using Mini.Engine.ECS.Components;
using Mini.Engine.Graphics.Cameras;
using Mini.Engine.Graphics.Diesel;

namespace Mini.Engine.Diesel;

[Service]
internal class DieselUpdateLoop
{
    private readonly ComponentLifeCycleSystem LifeCycleSystem;
    private readonly CameraController CameraController;
    private readonly CameraService CameraService;

    private readonly MetricService MetricService;

    public DieselUpdateLoop(ComponentLifeCycleSystem lifeCycleSystem, CameraController cameraController, CameraService cameraService, MetricService metricService)
    {
        this.LifeCycleSystem = lifeCycleSystem;
        this.CameraController = cameraController;
        this.CameraService = cameraService;
        this.MetricService = metricService;
    }

    public void Run(float elapsed)
    {
        var stopwatch = Stopwatch.StartNew();

        this.LifeCycleSystem.Process();

        ref var cameraTransform = ref this.CameraService.GetPrimaryCameraTransform();
        this.CameraController.Update(elapsed, ref cameraTransform.Current);

        this.MetricService.Update("DieselUpdateLoop.Run.Millis", (float)stopwatch.Elapsed.TotalMilliseconds);
    }
}
