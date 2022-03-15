using System.Collections.Generic;
using System.Linq;
using Mini.Engine.Configuration;
using Mini.Engine.Content;

namespace Mini.Engine.Scenes;

[Service]
public sealed class SceneManager
{
    private readonly LoadingScreen LoadingScreen;
    private readonly ContentManager Content;
    private readonly IReadOnlyList<IScene> Scenes;
    private int activeScene;

    public SceneManager(LoadingScreen loadingScreen, ContentManager content, IEnumerable<IScene> scenes)
    {
        this.LoadingScreen = loadingScreen;
        this.Content = content;
        this.Scenes = scenes.ToList();

        this.activeScene = -1;
    }

    public void Set(int index)
    {
        if (this.activeScene >= 0)
        {
            this.Content.Pop();
        }

        this.activeScene = index;
        var actions = this.Scenes[this.activeScene].Load();

        var title = this.Scenes[this.activeScene].Title;
        this.Content.Push($"Scene{title}");
        this.LoadingScreen.Load(actions, title);
    }
}
