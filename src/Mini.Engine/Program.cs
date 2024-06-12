using Mini.Engine.Configuration;
using Mini.Engine.Debugging;
using Mini.Engine.ECS.Components;

namespace Mini.Engine;

public class Program
{
    [STAThread]
    static void Main()
    {
        using var injector = new Injector();

        throw new Exception("TODO");
        // - Create a new GraphicsBootrapper that enables a Window, DirectX, and then RenderDoc if needed
        // - Then remove the code from GameBootstrapper
        // - Then create a new SceneBootstrapper
        // - - Add a way to switch scenes.
        // - - Start with a loading screen and main menu
        // - - Then create an ACTUAL game screen
        // - - Then a way to push/pop screens for example in case of a desync, pause etc..

        if (StartupArguments.EnableRenderDoc)
        {
            LoadRenderDoc(injector);
        }

        injector.RegisterContainer(typeof(ComponentContainer<>));
        var bootstrapper = injector.Get<GameBootstrapper>();
        bootstrapper.Run();
    }

    private static void LoadRenderDoc(Injector injector)
    {
        if (StartupArguments.EnableRenderDoc)
        {
            var loaded = RenderDoc.Load(out var renderDoc);
            if (loaded)
            {
                injector.Get<Services>().Register(renderDoc);
            }
            else
            {
                throw new Exception("Failed to load RenderDoc");
            }
        }
    }
}
