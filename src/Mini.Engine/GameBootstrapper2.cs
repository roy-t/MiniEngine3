using LightInject;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX;

namespace Mini.Engine;
public sealed class GameBootstrapper2
{
    private readonly IServiceFactory Factory;
    private readonly Device Device;
    private readonly LifetimeManager LifetimeManager;
    private readonly GameManager GameManager;
    private readonly SceneStack Scenes;

    public GameBootstrapper2(IServiceFactory factory, Device device, LifetimeManager lifetimeManager, GameManager gameManager, SceneStack scenes)
    {
        this.Factory = factory;
        this.Device = device;
        this.LifetimeManager = lifetimeManager;
        this.GameManager = gameManager;
        this.Scenes = scenes;
    }

    public void Bootstrap()
    {
        this.LifetimeManager.PushFrame(nameof(GameBootstrapper2));

        this.SetGraphicsSettings();

        var gameLoopType = Type.GetType(StartupArguments.GameLoopType, true, true)
            ?? throw new Exception($"Unable to find game loop {StartupArguments.GameLoopType}");

        var gameLoop = (IGameLoop)this.Factory.Create(gameLoopType);

        this.Scenes.Push(gameLoop);
        this.GameManager.Run();

        this.LifetimeManager.PopFrame(nameof(GameBootstrapper2));
    }

    private void SetGraphicsSettings()
    {
        this.Device.VSync = StartupArguments.EnableVSync;
    }
}
