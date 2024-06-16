using LightInject;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;

namespace Mini.Engine;
public sealed class GameBootstrapper
{
    private readonly IServiceFactory Factory;
    private readonly Device Device;
    private readonly GameManager GameManager;
    private readonly LoadingGameLoop LoadingGameLoop;
    private readonly SceneStack Scenes;

    public GameBootstrapper(IServiceFactory factory, Device device, GameManager gameManager, LoadingGameLoop loadingGameLoop, SceneStack scenes)
    {
        this.Factory = factory;
        this.Device = device;
        this.GameManager = gameManager;
        this.LoadingGameLoop = loadingGameLoop;
        this.Scenes = scenes;
    }

    public void Bootstrap()
    {
        this.SetGraphicsSettings();

        this.ConfigureLoadingScreen();

        this.GameManager.Run();
    }

    public void ConfigureLoadingScreen()
    {
        var gameLoopType = Type.GetType(StartupArguments.GameLoopType, true, true)
            ?? throw new Exception($"Unable to find game loop {StartupArguments.GameLoopType}");

        var dependencies = InjectableDependencies.CreateInitializationOrder(gameLoopType);
        foreach (var dependency in dependencies)
        {
            var action = new LoadAction(dependency.Name, () => this.Factory.GetInstance(dependency));
            this.LoadingGameLoop.Add(action);
        }

        this.LoadingGameLoop.Add(new LoadAction("Game Loop", () =>
        {
            var gameLoop = (IGameLoop)this.Factory.Create(gameLoopType);

            // Replace the loading screen with the actual game
            this.Scenes.ReplaceTop(gameLoop);
        }));

        this.Scenes.Push(this.LoadingGameLoop);
    }

    private void SetGraphicsSettings()
    {
        this.Device.VSync = StartupArguments.EnableVSync;
    }
}
