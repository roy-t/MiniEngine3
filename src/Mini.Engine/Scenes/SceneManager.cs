using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.ECS;
using Mini.Engine.Graphics;
using Mini.Engine.UI;

namespace Mini.Engine.Scenes;

[Service]
public sealed class SceneManager
{
    private readonly LifetimeManager LifetimeManager;
    private readonly LoadingScreen LoadingScreen;    
    private readonly ECSAdministrator Administrator;
    private readonly FrameService FrameService;
    private int activeScene;
    private int nextScene;

    public SceneManager(LifetimeManager lifetimeManager, LoadingScreen loadingScreen, ECSAdministrator administrator, FrameService frameService, IEnumerable<IScene> scenes)
    {
        this.LifetimeManager = lifetimeManager;
        this.LoadingScreen = loadingScreen;        
        this.Administrator = administrator;
        this.FrameService = frameService;
        this.Scenes = scenes.ToList();

        this.activeScene = -1;
        this.nextScene = -1;
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

    private void ChangeScene(int index)
    {
        if (this.activeScene >= 0)
        {
            this.Administrator.RemoveAll();
            this.LifetimeManager.PopFrame();            
        }        

        this.activeScene = index;
        var title = this.Scenes[this.activeScene].Title;

        this.LifetimeManager.PushFrame($"Scene: {title}");
        var actions = this.Scenes[this.activeScene].Load();        
        this.LoadingScreen.Load(actions, title);

        this.FrameService.InitializePrimaryCamera();
    }
}
