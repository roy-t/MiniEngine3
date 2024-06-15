using LightInject;

namespace Mini.Engine.Configuration;
/// <summary>
/// Wrapper around ServiceContainer, which doesn't include IDispose to prevent double disposes
/// </summary>
internal sealed class ServiceFactoryAdapter : IServiceFactory
{
    private readonly ServiceContainer Container;

    public ServiceFactoryAdapter(ServiceContainer container)
    {
        this.Container = container;
    }

    public Scope BeginScope()
    {
        return this.Container.BeginScope();
    }

    public object Create(Type serviceType)
    {
        return this.Container.Create(serviceType);
    }

    public IEnumerable<object> GetAllInstances(Type serviceType)
    {
        return this.Container.GetAllInstances(serviceType);
    }

    public object GetInstance(Type serviceType)
    {
        return this.Container.GetInstance(serviceType);
    }

    public object GetInstance(Type serviceType, object[] arguments)
    {
        return this.Container.GetInstance(serviceType, arguments);
    }

    public object GetInstance(Type serviceType, string serviceName, object[] arguments)
    {
        return this.Container.GetInstance(serviceType, serviceName, arguments);
    }

    public object GetInstance(Type serviceType, string serviceName)
    {
        return this.Container.GetInstance(serviceType, serviceName);
    }

    public object TryGetInstance(Type serviceType)
    {
        return this.Container.TryGetInstance(serviceType);
    }

    public object TryGetInstance(Type serviceType, string serviceName)
    {
        return this.Container.TryGetInstance(serviceType, serviceName);
    }
}
