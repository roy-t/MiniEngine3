﻿using Mini.Engine.Configuration;
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
        var bootstrapper = injector.Get<GameBootstrapper>();
        bootstrapper.Run();
    }
}
