using System;
using Mini.Engine.Configuration;
using Mini.Engine.ECS.Components;

namespace Mini.Engine
{
    public class Program
    {
        [STAThread]
        static void Main()
        {
            using var injector = new Injector();
            injector.RegisterContainer(typeof(LookupArray<>));
            var bootstrapper = injector.Get<GameBootstrapper>();
            bootstrapper.Run();
        }
    }
}
