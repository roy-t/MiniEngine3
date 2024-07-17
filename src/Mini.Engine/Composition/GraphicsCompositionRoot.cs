using LightInject;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.Debugging;
using Mini.Engine.DirectX;
using Mini.Engine.Windows;

namespace Mini.Engine.Composition;
public sealed class GraphicsCompositionRoot : ICompositionRoot
{
    public void Compose(IServiceRegistry registry)
    {
        registry.Register<Device>(factory =>
        {
            var window = factory.GetInstance<Win32Window>();
            var lifeTimeManager = factory.GetInstance<LifetimeManager>();
            return new Device(window.Handle, window.Width, window.Height, lifeTimeManager);
        });
        registry.Initialize<Win32Window>((_, _) =>
        {
            if (StartupArguments.EnableRenderDoc)
            {
                var loaded = RenderDoc.Load(out var renderdoc);
                if (loaded)
                {
                    registry.RegisterInstance(renderdoc);
                }
                else
                {
                    throw new Exception("Failed to load RenderDoc");
                }
            }
        });
    }
}
