using LightInject;
using Mini.Engine.Debugging;

namespace Mini.Engine.Composition;
public sealed class DebugCompositionRoot : ICompositionRoot
{
    public void Compose(IServiceRegistry serviceRegistry)
    {
        serviceRegistry.Register<MetricService>();
    }
}
