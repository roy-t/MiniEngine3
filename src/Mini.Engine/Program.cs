using Mini.Engine.Configuration;

namespace Mini.Engine
{
    public class Program
    {
        [STAThread]
        static void Main()
        {
            using var injector = new Injector();
            var bootstrapper = injector.Get<GameBootstrapper>();
            bootstrapper.Run();
        }
    }
}
