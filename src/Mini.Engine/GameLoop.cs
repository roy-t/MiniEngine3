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

    private readonly RenderHelper FXAARenderer;
    private readonly SceneManager SceneManager;
    private readonly FrameService FrameService;
    private readonly DebugFrameService DebugFrameService;
    private readonly CameraController CameraController;
    private readonly ContentManager Content;
    private readonly ParallelPipeline RenderPipeline;
    private readonly ParallelPipeline DebugPipeline;
    public GameLoop(Device device, RenderHelper helper, SceneManager sceneManager, FrameService frameService, DebugFrameService debugFrameService, CameraController cameraController, RenderPipelineBuilder renderBuilder, DebugPipelineBuilder debugBuilder, ContentManager content)
    {
        this.Device = device;
        this.FXAARenderer = helper;
        this.SceneManager = sceneManager;
        this.FrameService = frameService;
        this.DebugFrameService = debugFrameService;
        this.CameraController = cameraController;
        this.Content = content;

        content.Push("RenderPipeline");
        this.RenderPipeline = renderBuilder.Build();

        content.Push("DebugPipeline");
        this.DebugPipeline = debugBuilder.Build();

        content.Push("Game");
        this.SceneManager.Set(0);
    }

    public void Update(float time, float elapsed)
    {
        this.SceneManager.CheckChangeScene();

        this.Content.ReloadChangedContent();
        this.CameraController.Update(this.FrameService.Camera, elapsed);        
    }

    public void Draw(float alpha)
    {
        this.FrameService.Alpha = alpha;
        this.RenderPipeline.Frame();

        this.Device.ImmediateContext.OM.SetRenderTargetToBackBuffer();

        this.FXAARenderer.RenderFXAA(this.Device.ImmediateContext, this.FrameService.LBuffer.Light, 0, 0, this.Device.Width, this.Device.Height);

        if (this.DebugFrameService.EnableDebugOverlay)
        {
            this.DebugPipeline.Frame();

            if (this.DebugFrameService.RenderToViewport)
            {
                this.Device.ImmediateContext.OM.SetRenderTargetToBackBuffer();
                this.FXAARenderer.Render(this.Device.ImmediateContext, this.DebugFrameService.DebugOverlay, 0, 0, this.Device.Width, this.Device.Height);
            }
        }
    }

    public void Resize(int width, int height)
    {
        this.FrameService.Resize(this.Device);
    }

    public void Dispose()
    {
        this.RenderPipeline.Dispose();
    }
}
