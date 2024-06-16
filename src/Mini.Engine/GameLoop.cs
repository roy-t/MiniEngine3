using System.Diagnostics;
using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.Debugging;
using Mini.Engine.DirectX;
using Mini.Engine.Graphics;
using Mini.Engine.Graphics.PostProcessing;
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

    private readonly MetricService MetricService;
    private readonly LifetimeManager LifetimeManager;
    private readonly EditorState EditorState;
    private readonly PresentationHelper Presenter;
    private readonly Scenes.SceneManager SceneManager;
    private readonly FrameService FrameService;
    private readonly ContentManager Content;

    private readonly RenderPipeline RenderPipeline;
    private readonly UpdatePipeline UpdatePipeline;

    private bool enableUI;
    private readonly Stopwatch Stopwatch;

    public GameLoop(Device device, EditorUserInterface userInterface, InputService inputService, LifetimeManager lifetimeManager, EditorState editorState, PresentationHelper presenter, Scenes.SceneManager sceneManager, FrameService frameService, ContentManager content, MetricService metricService, RenderPipeline renderPipelineV2, UpdatePipeline updatePipelineV2)
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
        this.Content = content;

        this.LifetimeManager.PushFrame(nameof(GameLoop));

        this.EditorState.Restore();
        this.SceneManager.Set(this.EditorState.PreferredScene);

        this.enableUI = !StartupArguments.NoUi;
        this.MetricService = metricService;

        this.Stopwatch = new Stopwatch();
        this.RenderPipeline = renderPipelineV2;
        this.UpdatePipeline = updatePipelineV2;
    }

    public void Simulate()
    {
        this.Stopwatch.Restart();
        this.SceneManager.CheckChangeScene();
        this.Content.ReloadChangedContent();

        this.EditorState.Update();
        this.UpdatePipeline.Run();

        this.MetricService.Update("GameLoop.Update.Millis", (float)this.Stopwatch.Elapsed.TotalMilliseconds);
    }

    public void HandleInput(float elapsedRealWorldTime)
    {
        while (this.InputService.ProcessEvents(this.Keyboard))
        {
            if (this.Keyboard.Pressed(F1))
            {
                this.enableUI = !this.enableUI;
            }
        }
    }

    public void Frame(float alpha, float elapsedRealWorldTime)
    {
        this.Stopwatch.Restart();

        this.UserInterface.NewFrame();

        this.FrameService.ElapsedRealWorldTime = elapsedRealWorldTime;
        this.FrameService.Alpha = alpha;
        this.FrameService.PBuffer.Swap(ref this.FrameService.GetPrimaryCamera());
        ClearBuffersSystem.Clear(this.Device.ImmediateContext, this.FrameService);

        var output = this.Device.Viewport;
        this.RenderPipeline.Run(in output, in output, alpha);
        this.Presenter.ToneMapAndPresent(this.Device.ImmediateContext, this.FrameService.PBuffer.CurrentColor);

        if (this.enableUI)
        {
            this.UserInterface.Render();
        }

        this.MetricService.Update("GameLoop.Draw.Millis", (float)this.Stopwatch.Elapsed.TotalMilliseconds);
    }

    public void Resize(int width, int height)
    {
        this.FrameService.Resize(this.Device);
        this.UserInterface.Resize(width, height);
    }

    public void Dispose()
    {
        this.EditorState.Save();
    }
}
