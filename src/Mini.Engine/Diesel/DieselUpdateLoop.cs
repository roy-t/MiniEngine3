using Mini.Engine.Configuration;
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

    public DieselUpdateLoop(ComponentLifeCycleSystem lifeCycleSystem, CameraController cameraController, CameraService cameraService)
    {
        this.LifeCycleSystem = lifeCycleSystem;
        this.CameraController = cameraController;
        this.CameraService = cameraService;
    }

    public void Run(float elapsed)
    {
        this.LifeCycleSystem.Process();

        ref var cameraTransform = ref this.CameraService.GetPrimaryCameraTransform();
        this.CameraController.Update(elapsed, ref cameraTransform.Current);
    }
}
