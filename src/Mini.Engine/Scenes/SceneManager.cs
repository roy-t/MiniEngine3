using System.Collections.Generic;
using System.Linq;
using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Entities;

namespace Mini.Engine.Scenes;

[Service]
public sealed class SceneManager
{
    private readonly LoadingScreen LoadingScreen;
    private readonly ContentManager Content;
    private readonly ECSAdministrator Administrator;
    private int activeScene;
    private int nextScene;

    public SceneManager(LoadingScreen loadingScreen, ContentManager content, ECSAdministrator administrator, IEnumerable<IScene> scenes)
    {
        this.LoadingScreen = loadingScreen;
        this.Content = content;
        this.Administrator = administrator;
        this.Scenes = scenes.ToList();

        this.activeScene = -1;
        this.nextScene = -1;
    }

    public IReadOnlyList<IScene> Scenes { get; }

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
            this.Content.Pop();
            this.Administrator.RemoveAll();
        }

        this.activeScene = index;
        var actions = this.Scenes[this.activeScene].Load();

        var title = this.Scenes[this.activeScene].Title;
        this.Content.Push($"Scene{title}");
        this.LoadingScreen.Load(actions, title);
    }    
}
