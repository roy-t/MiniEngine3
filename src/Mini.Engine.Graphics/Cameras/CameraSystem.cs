using Mini.Engine.Configuration;

namespace Mini.Engine.Graphics.Cameras;

[Service]
public sealed class CameraSystem
{
    private readonly CameraController CameraController;
    private readonly FrameService FrameService;    

    public CameraSystem(CameraController cameraController, FrameService frameService)
    {
        this.CameraController = cameraController;
        this.FrameService = frameService;
    }

    public void Update()
    {
        ref var cameraTransform = ref this.FrameService.GetPrimaryCameraTransform();
        this.CameraController.Update(ref cameraTransform.Current);
    }
}
