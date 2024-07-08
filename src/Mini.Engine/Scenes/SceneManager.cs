using Mini.Engine.Configuration;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.ECS;
using Mini.Engine.Graphics;

namespace Mini.Engine.Scenes;

[Service]
internal sealed class SceneManager
{
    private readonly LifetimeManager LifetimeManager;
    private readonly LoadingGameLoop LoadingScreen;
    private readonly ECSAdministrator Administrator;
    private readonly FrameService FrameService;
    private int activeScene;
    private int nextScene;

    private LifeTimeFrame? frame;

    public SceneManager(LifetimeManager lifetimeManager, LoadingGameLoop loadingScreen, ECSAdministrator administrator, FrameService frameService, IEnumerable<IScene> scenes)
    {
        this.LifetimeManager = lifetimeManager;
        this.LoadingScreen = loadingScreen;
        this.Administrator = administrator;
        this.FrameService = frameService;
        this.Scenes = scenes.ToList();

        this.activeScene = -1;
        this.nextScene = -1;

        this.frame = null;
    }

    public IReadOnlyList<IScene> Scenes { get; }

    public int ActiveScene => this.activeScene;

    public void CheckChangeScene()
    {
        if (this.nextScene != this.activeScene)
        {
            this.ChangeScene(this.nextScene);
        }
    }

    public void Set(int index)
    {
        this.nextScene = index;
    }

    public void ClearScene()
    {
        if (this.frame != null)
        {
            this.Administrator.RemoveAll();
            this.LifetimeManager.PopFrame(this.frame);
            this.frame = null;
        }
    }

    private void ChangeScene(int index)
    {
        this.ClearScene();
        this.FrameService.InitializePrimaryCamera();

        this.activeScene = index;

        this.frame = this.LifetimeManager.PushFrame();
        var actions = this.Scenes[this.activeScene].Load();

        this.LoadingScreen.PushLoadPop(actions);
    }
}
