using LightInject;
using Mini.Engine.Windows;

namespace Mini.Engine.Composition;
public sealed class WindowsCompositionRoot : ICompositionRoot
{
    public void Compose(IServiceRegistry registry)
    {
        registry.Register<Win32Window>((factory) => Win32Application.Initialize("Mini.Engine"));
        registry.Register<SimpleInputService>();
    }
}
