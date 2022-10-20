using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.Controllers;
using Mini.Engine.DirectX;
using Mini.Engine.ECS.Pipeline;
using Mini.Engine.Graphics;
using Mini.Engine.Graphics.PostProcessing;
using Mini.Engine.Graphics.Vegetation;
using Mini.Engine.Scenes;

namespace Mini.Engine;

[Service]
internal sealed class GameLoop : IGameLoop
{
    private readonly Device Device;

    private readonly PresentationHelper Presenter;
    private readonly SceneManager SceneManager;
    private readonly FrameService FrameService;
    private readonly DebugFrameService DebugFrameService;
    private readonly CameraController CameraController;
    private readonly ContentManager Content;
    private readonly Mini.Engine.Content.v2.ContentManager ContentV2;
    private readonly GrassSystem GrassSystem; // TODO: move this to a smarter place?
    private readonly ParallelPipeline RenderPipeline;
    private readonly ParallelPipeline DebugPipeline;
    public GameLoop(Device device, PresentationHelper presenter, SceneManager sceneManager, FrameService frameService, DebugFrameService debugFrameService, CameraController cameraController, RenderPipelineBuilder renderBuilder, DebugPipelineBuilder debugBuilder, ContentManager content, GrassSystem grassSystem, Content.v2.ContentManager contentV2)
    {
        this.Device = device;
        this.Presenter = presenter;
        this.SceneManager = sceneManager;
        this.FrameService = frameService;
        this.DebugFrameService = debugFrameService;
        this.CameraController = cameraController;
        this.Content = content;
        this.GrassSystem = grassSystem;
        content.Push("RenderPipeline");
        this.RenderPipeline = renderBuilder.Build();

        content.Push("DebugPipeline");
        this.DebugPipeline = debugBuilder.Build();

        content.Push("Game");
        this.SceneManager.Set(0);
        this.ContentV2 = contentV2;
    }

    public void Update(float time, float elapsed)
    {
        this.SceneManager.CheckChangeScene();

        // TODO while working on v2 hot reloading for v1 is broken!
        //this.Content.ReloadChangedContent(); 
        this.ContentV2.ReloadChangedContent();

        ref var camera = ref this.FrameService.GetPrimaryCamera();
        ref var cameraTransform = ref this.FrameService.GetPrimaryCameraTransform();        

        this.CameraController.Update(ref camera, ref cameraTransform, elapsed);

        this.GrassSystem.UpdateWind(elapsed);
    }

    public void Draw(float alpha)
    {
        this.FrameService.Alpha = alpha;
        this.RenderPipeline.Frame();

        this.Presenter.ToneMapAndPresent(this.Device.ImmediateContext, this.FrameService.PBuffer.Current);        

        if (this.DebugFrameService.EnableDebugOverlay)
        {
            this.DebugPipeline.Frame();

            if (this.DebugFrameService.RenderToViewport)
            {
                this.Device.ImmediateContext.OM.SetRenderTargetToBackBuffer();
                this.Presenter.Present(this.Device.ImmediateContext, this.DebugFrameService.DebugOverlay);
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
