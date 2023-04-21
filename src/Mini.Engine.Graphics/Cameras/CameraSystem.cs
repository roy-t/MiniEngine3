using Mini.Engine.Configuration;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.ECS.Systems;

namespace Mini.Engine.Graphics.Cameras;

[Service]
public sealed partial class CameraSystem : ISystem
{
    private readonly CameraController CameraController;
    private readonly FrameService FrameService;

    public CameraSystem(CameraController cameraController, FrameService frameService)
    {
        this.CameraController = cameraController;
        this.FrameService = frameService;
    }

    public void OnSet() { }

    [Process]
    public void Update()
    {
        var elapsed = this.FrameService.Elapsed;
        ref var cameraTransform = ref this.FrameService.GetPrimaryCameraTransform();

        this.CameraController.Update(elapsed, ref cameraTransform.Current);
    }

    public void OnUnSet() { }
}
