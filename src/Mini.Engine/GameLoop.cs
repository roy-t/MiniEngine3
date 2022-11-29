using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX;
using Mini.Engine.ECS.Pipeline;
using Mini.Engine.Graphics;
using Mini.Engine.Graphics.PostProcessing;
using Mini.Engine.Scenes;
using Mini.Engine.UI;

namespace Mini.Engine;

[Service]
internal sealed class GameLoop : IGameLoop
{
    private readonly Device Device;
    private readonly LifetimeManager LifetimeManager;
    private readonly EditorState EditorState;
    private readonly PresentationHelper Presenter;
    private readonly SceneManager SceneManager;
    private readonly FrameService FrameService;
    private readonly DebugFrameService DebugFrameService;
    private readonly ContentManager Content;
    private readonly ParallelPipeline UpdatePipeline;
    private readonly ParallelPipeline RenderPipeline;
    private readonly ParallelPipeline DebugPipeline;
    public GameLoop(Device device, LifetimeManager lifetimeManager, EditorState editorState, PresentationHelper presenter, SceneManager sceneManager, FrameService frameService, DebugFrameService debugFrameService, UpdatePipelineBuilder updatePipelineBuilder, RenderPipelineBuilder renderBuilder, DebugPipelineBuilder debugBuilder, ContentManager content)
    {
        this.Device = device;
        this.LifetimeManager = lifetimeManager;
        this.EditorState = editorState;
        this.Presenter = presenter;
        this.SceneManager = sceneManager;
        this.FrameService = frameService;
        this.DebugFrameService = debugFrameService;
        this.Content = content;

        this.LifetimeManager.PushFrame("Pipelines");
        this.UpdatePipeline = updatePipelineBuilder.Build();        
        this.RenderPipeline = renderBuilder.Build();        
        this.DebugPipeline = debugBuilder.Build();

        this.LifetimeManager.PushFrame("Game");

        this.EditorState.Restore();
        this.SceneManager.Set(this.EditorState.PreferredScene);
    }

    public void Update(float time, float elapsed)
    {
        this.FrameService.Elapsed = elapsed;
        this.SceneManager.CheckChangeScene();
        this.Content.ReloadChangedContent();
        this.EditorState.Update();        
                        
        this.UpdatePipeline.Frame();      
    }

    public void Draw(float alpha)
    {
        this.FrameService.Alpha = alpha;
        this.RenderPipeline.Frame();

        this.Presenter.ToneMapAndPresent(this.Device.ImmediateContext, this.FrameService.PBuffer.CurrentColor);        

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
        this.EditorState.Save();
        this.RenderPipeline.Dispose();
    }
}
