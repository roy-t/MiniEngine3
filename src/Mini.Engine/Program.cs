using LightInject;
using Mini.Engine.Composition;
using Mini.Engine.Configuration;
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

        var bootstrapper = injector.Factory.Create<GameBootstrapper>();
        bootstrapper.Run();


        //throw new Exception("TODO");
        //// - Create a new GraphicsBootrapper that enables a Window, DirectX, and then RenderDoc if needed
        //// - Then remove the code from GameBootstrapper
        //// - Then create a new SceneBootstrapper
        //// - - Add a way to switch scenes.
        //// - - Start with a loading screen and main menu
        //// - - Then create an ACTUAL game screen
        //// - - Then a way to push/pop screens for example in case of a desync, pause etc..

    }
}
