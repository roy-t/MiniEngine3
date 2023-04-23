using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX;
using Mini.Engine.ECS.Pipeline;
using Mini.Engine.Graphics;
using Mini.Engine.Graphics.PostProcessing;
using Mini.Engine.Scenes;
using Mini.Engine.UI;
using Mini.Engine.Windows;
using static Windows.Win32.UI.Input.KeyboardAndMouse.VIRTUAL_KEY;

namespace Mini.Engine;

[Service]
internal sealed class GameLoop : IGameLoop
{
    private static readonly ushort F1 = InputService.GetScanCode(VK_F1);

    private readonly Device Device;
    private readonly EditorUserInterface UserInterface;
    private readonly InputService InputService;
    private readonly Keyboard Keyboard;

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

    private bool enableUI;

    public GameLoop(Device device, EditorUserInterface userInterface, InputService inputService, LifetimeManager lifetimeManager, EditorState editorState, PresentationHelper presenter, SceneManager sceneManager, FrameService frameService, DebugFrameService debugFrameService, UpdatePipelineBuilder updatePipelineBuilder, RenderPipelineBuilder renderBuilder, DebugPipelineBuilder debugBuilder, ContentManager content)
    {
        this.Device = device;
        this.UserInterface = userInterface;
        this.InputService = inputService;
        this.Keyboard = new Keyboard();

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

        this.enableUI = !StartupArguments.NoUi;
    }

    public void Update(float time, float elapsed)
    {
        this.FrameService.Elapsed = elapsed;

        this.UserInterface.NewFrame(elapsed);

        this.SceneManager.CheckChangeScene();
        this.Content.ReloadChangedContent();
        this.EditorState.Update();

        this.UpdatePipeline.Frame();

        while (this.InputService.ProcessEvents(this.Keyboard))
        {
            if (this.Keyboard.Pressed(F1))
            {
                this.enableUI = !this.enableUI;
            }
        }
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

        if (this.enableUI)
        {
            this.UserInterface.Render();
        }
    }

    public void Resize(int width, int height)
    {
        this.FrameService.Resize(this.Device);
        this.UserInterface.Resize(width, height);
    }

    public void Dispose()
    {
        this.SceneManager.ClearScene();

        this.LifetimeManager.PopFrame(); // Game

        this.UpdatePipeline.Dispose();
        this.RenderPipeline.Dispose();
        this.DebugPipeline.Dispose();

        this.LifetimeManager.PopFrame(); // Pipelines

        this.EditorState.Save();
    }
}
