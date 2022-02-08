using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.Controllers;
using Mini.Engine.DirectX;
using Mini.Engine.ECS.Pipeline;
using Mini.Engine.Graphics;
using Mini.Engine.Scenes;

namespace Mini.Engine;

[Service]
internal sealed class GameLoop : IGameLoop
{
    private readonly Device Device;

    private readonly RenderHelper Helper;
    private readonly SceneManager SceneManager;
    private readonly FrameService FrameService;
    private readonly CameraController CameraController;
    private readonly ContentManager Content;
    private readonly ParallelPipeline Pipeline;    
    public GameLoop(Device device, RenderHelper helper, SceneManager sceneManager, FrameService frameService, CameraController cameraController, RenderPipelineBuilder builder, ContentManager content)
    {
        this.Device = device;
        this.Helper = helper;
        this.SceneManager = sceneManager;
        this.FrameService = frameService;
        this.CameraController = cameraController;
        this.Content = content;        

        content.Push("RenderPipeline");
        this.Pipeline = builder.Build();

        this.SceneManager.Set(0);
    }

    public void Update(float time, float elapsed)
    {
        this.Content.ReloadChangedContent();
        this.CameraController.Update(this.FrameService.Camera, elapsed);
    }

    public void Draw(float alpha)
    {
        this.FrameService.Alpha = alpha;
        this.Pipeline.Frame();

        this.Helper.RenderToViewPort(this.Device.ImmediateContext, this.FrameService.LBuffer.Light, 0, 0, this.Device.Width, this.Device.Height);
    }

    public void Dispose()
    {
        this.Pipeline.Dispose();
    }
}
