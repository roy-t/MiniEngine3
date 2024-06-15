using LightInject;
using Mini.Engine.Core.Lifetime;

namespace Mini.Engine.Composition;
public sealed class CoreCompositionRoot : ICompositionRoot
{
    public void Compose(IServiceRegistry registry)
    {
        registry.Register<LifetimeManager>();
    }
}
