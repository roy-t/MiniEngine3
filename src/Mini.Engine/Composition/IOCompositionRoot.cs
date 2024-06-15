using LightInject;
using Mini.Engine.IO;
using Serilog;

namespace Mini.Engine.Composition;
public sealed class IOCompositionRoot : ICompositionRoot
{
    public void Compose(IServiceRegistry registry)
    {
        registry.Register<IVirtualFileSystem>((factory) =>
            new DiskFileSystem(factory.GetInstance<ILogger>(), StartupArguments.ContentRoot));
    }
}
