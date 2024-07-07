using LightInject;
using Mini.Engine.Composition;
using Mini.Engine.Configuration;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.ECS.Components;

namespace Mini.Engine;

public class Program
{
    [STAThread]
    static void Main()
    {
        using var injector = new Injector();
        injector.RegisterContainer(typeof(ComponentContainer<>));

        var registry = injector.Registry;
        registry.RegisterFrom<CoreCompositionRoot>();
        registry.RegisterFrom<IOCompositionRoot>();
        registry.RegisterFrom<WindowsCompositionRoot>();
        registry.RegisterFrom<GraphicsCompositionRoot>();
        registry.RegisterFrom<DebugCompositionRoot>();

        var lifeTimeManager = injector.Factory.GetInstance<LifetimeManager>();
        var programFrame = lifeTimeManager.PushFrame();
        {
            var bootstrapper = injector.Factory.Create<GameBootstrapper>();
            bootstrapper.Bootstrap();
        }
        lifeTimeManager.PopFrame(programFrame);
    }
}
