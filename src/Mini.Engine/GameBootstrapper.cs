using LightInject;
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

        this.LoadingGameLoop.PushLoadReplace(gameLoopType);
    }

    private void SetGraphicsSettings()
    {
        this.Device.VSync = StartupArguments.EnableVSync;
    }
}
