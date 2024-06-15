using System.Reflection;
using LightInject;

namespace Mini.Engine.Configuration;
/// <summary>
/// Wrapper around ServiceContainer, which doesn't include IDispose to prevent double disposes
/// </summary>
internal sealed class ServiceRegistryShim : IServiceRegistry
{
    private readonly ServiceContainer Container;

    public ServiceRegistryShim(ServiceContainer container)
    {
        this.Container = container;
    }

    public IEnumerable<ServiceRegistration> AvailableServices => this.Container.AvailableServices;

    public IServiceRegistry Decorate(Type serviceType, Type decoratorType, Func<ServiceRegistration, bool> predicate)
    {
        return this.Container.Decorate(serviceType, decoratorType, predicate);
    }

    public IServiceRegistry Decorate(Type serviceType, Type decoratorType)
    {
        return this.Container.Decorate(serviceType, decoratorType);
    }

    public IServiceRegistry Decorate<TService, TDecorator>() where TDecorator : TService
    {
        return this.Container.Decorate<TService, TDecorator>();
    }

    public IServiceRegistry Decorate<TService>(Func<IServiceFactory, TService, TService> factory)
    {
        return this.Container.Decorate(factory);
    }

    public IServiceRegistry Decorate(DecoratorRegistration decoratorRegistration)
    {
        return this.Container.Decorate(decoratorRegistration);
    }

    public IServiceRegistry Initialize(Func<ServiceRegistration, bool> predicate, Action<IServiceFactory, object> processor)
    {
        return this.Container.Initialize(predicate, processor);
    }

    public IServiceRegistry Override(Func<ServiceRegistration, bool> serviceSelector, Func<IServiceFactory, ServiceRegistration, ServiceRegistration> serviceRegistrationFactory)
    {
        return this.Container.Override(serviceSelector, serviceRegistrationFactory);
    }

    public IServiceRegistry Register(Type serviceType, Type implementingType)
    {
        return this.Container.Register(serviceType, implementingType);
    }

    public IServiceRegistry Register(Type serviceType, Type implementingType, ILifetime lifetime)
    {
        return this.Container.Register(serviceType, implementingType, lifetime);
    }

    public IServiceRegistry Register(Type serviceType, Type implementingType, string serviceName)
    {
        return this.Container.Register(serviceType, implementingType, serviceName);
    }

    public IServiceRegistry Register(Type serviceType, Type implementingType, string serviceName, ILifetime lifetime)
    {
        return this.Container.Register(serviceType, implementingType, serviceName, lifetime);
    }

    public IServiceRegistry Register<TService, TImplementation>() where TImplementation : TService
    {
        return this.Container.Register<TService, TImplementation>();
    }

    public IServiceRegistry Register<TService, TImplementation>(ILifetime lifetime) where TImplementation : TService
    {
        return this.Container.Register<TService, TImplementation>(lifetime);
    }

    public IServiceRegistry Register<TService, TImplementation>(string serviceName) where TImplementation : TService
    {
        return this.Container.Register<TService, TImplementation>(serviceName);
    }

    public IServiceRegistry Register<TService, TImplementation>(string serviceName, ILifetime lifetime) where TImplementation : TService
    {
        return this.Container.Register<TService, TImplementation>(serviceName, lifetime);
    }

    public IServiceRegistry Register<TService>()
    {
        return this.Container.Register<TService>();
    }

    public IServiceRegistry Register<TService>(ILifetime lifetime)
    {
        return this.Container.Register<TService>(lifetime);
    }

    public IServiceRegistry Register(Type serviceType)
    {
        return this.Container.Register(serviceType);
    }

    public IServiceRegistry Register(Type serviceType, ILifetime lifetime)
    {
        return this.Container.Register(serviceType, lifetime);
    }

    public IServiceRegistry Register<TService>(Func<IServiceFactory, TService> factory)
    {
        return this.Container.Register(factory);
    }

    public IServiceRegistry Register<T, TService>(Func<IServiceFactory, T, TService> factory)
    {
        return this.Container.Register(factory);
    }

    public IServiceRegistry Register<T, TService>(Func<IServiceFactory, T, TService> factory, string serviceName)
    {
        return this.Container.Register(factory, serviceName);
    }

    public IServiceRegistry Register<T1, T2, TService>(Func<IServiceFactory, T1, T2, TService> factory)
    {
        return this.Container.Register(factory);
    }

    public IServiceRegistry Register<T1, T2, TService>(Func<IServiceFactory, T1, T2, TService> factory, string serviceName)
    {
        return this.Container.Register(factory, serviceName);
    }

    public IServiceRegistry Register<T1, T2, T3, TService>(Func<IServiceFactory, T1, T2, T3, TService> factory)
    {
        return this.Container.Register(factory);
    }

    public IServiceRegistry Register<T1, T2, T3, TService>(Func<IServiceFactory, T1, T2, T3, TService> factory, string serviceName)
    {
        return this.Container.Register(factory, serviceName);
    }

    public IServiceRegistry Register<T1, T2, T3, T4, TService>(Func<IServiceFactory, T1, T2, T3, T4, TService> factory)
    {
        return this.Container.Register(factory);
    }

    public IServiceRegistry Register<T1, T2, T3, T4, TService>(Func<IServiceFactory, T1, T2, T3, T4, TService> factory, string serviceName)
    {
        return this.Container.Register(factory, serviceName);
    }

    public IServiceRegistry Register<TService>(Func<IServiceFactory, TService> factory, ILifetime lifetime)
    {
        return this.Container.Register(factory, lifetime);
    }

    public IServiceRegistry Register<TService>(Func<IServiceFactory, TService> factory, string serviceName)
    {
        return this.Container.Register(factory, serviceName);
    }

    public IServiceRegistry Register<TService>(Func<IServiceFactory, TService> factory, string serviceName, ILifetime lifetime)
    {
        return this.Container.Register(factory, serviceName, lifetime);
    }

    public IServiceRegistry Register(ServiceRegistration serviceRegistration)
    {
        return this.Container.Register(serviceRegistration);
    }

    public IServiceRegistry RegisterAssembly(Assembly assembly)
    {
        return this.Container.RegisterAssembly(assembly);
    }

    public IServiceRegistry RegisterAssembly(Assembly assembly, Func<Type, Type, bool> shouldRegister)
    {
        return this.Container.RegisterAssembly(assembly, shouldRegister);
    }

    public IServiceRegistry RegisterAssembly(Assembly assembly, Func<ILifetime> lifetimeFactory)
    {
        return this.Container.RegisterAssembly(assembly, lifetimeFactory);
    }

    public IServiceRegistry RegisterAssembly(Assembly assembly, Func<ILifetime> lifetimeFactory, Func<Type, Type, bool> shouldRegister)
    {
        return this.Container.RegisterAssembly(assembly, lifetimeFactory, shouldRegister);
    }

    public IServiceRegistry RegisterAssembly(Assembly assembly, Func<ILifetime> lifetimeFactory, Func<Type, Type, bool> shouldRegister, Func<Type, Type, string> serviceNameProvider)
    {
        return this.Container.RegisterAssembly(assembly, lifetimeFactory, shouldRegister, serviceNameProvider);
    }

    public IServiceRegistry RegisterAssembly(string searchPattern)
    {
        return this.Container.RegisterAssembly(searchPattern);
    }

    public IServiceRegistry RegisterConstructorDependency<TDependency>(Func<IServiceFactory, ParameterInfo, TDependency> factory)
    {
        return this.Container.RegisterConstructorDependency(factory);
    }

    public IServiceRegistry RegisterConstructorDependency<TDependency>(Func<IServiceFactory, ParameterInfo, object[], TDependency> factory)
    {
        return this.Container.RegisterConstructorDependency(factory);
    }

    public IServiceRegistry RegisterFallback(Func<Type, string, bool> predicate, Func<ServiceRequest, object> factory)
    {
        return this.Container.RegisterFallback(predicate, factory);
    }

    public IServiceRegistry RegisterFallback(Func<Type, string, bool> predicate, Func<ServiceRequest, object> factory, ILifetime lifetime)
    {
        return this.Container.RegisterFallback(predicate, factory, lifetime);
    }

    public IServiceRegistry RegisterFrom<TCompositionRoot>() where TCompositionRoot : ICompositionRoot, new()
    {
        return this.Container.RegisterFrom<TCompositionRoot>();
    }

    public IServiceRegistry RegisterFrom<TCompositionRoot>(TCompositionRoot compositionRoot) where TCompositionRoot : ICompositionRoot
    {
        return this.Container.RegisterFrom(compositionRoot);
    }

    public IServiceRegistry RegisterInstance<TService>(TService instance)
    {
        return this.Container.RegisterInstance(instance);
    }

    public IServiceRegistry RegisterInstance<TService>(TService instance, string serviceName)
    {
        return this.Container.RegisterInstance(instance, serviceName);
    }

    public IServiceRegistry RegisterInstance(Type serviceType, object instance)
    {
        return this.Container.RegisterInstance(serviceType, instance);
    }

    public IServiceRegistry RegisterInstance(Type serviceType, object instance, string serviceName)
    {
        return this.Container.RegisterInstance(serviceType, instance, serviceName);
    }

    public IServiceRegistry RegisterOrdered(Type serviceType, Type[] implementingTypes, Func<Type, ILifetime> lifetimeFactory)
    {
        return this.Container.RegisterOrdered(serviceType, implementingTypes, lifetimeFactory);
    }

    public IServiceRegistry RegisterOrdered(Type serviceType, Type[] implementingTypes, Func<Type, ILifetime> lifeTimeFactory, Func<int, string> serviceNameFormatter)
    {
        return this.Container.RegisterOrdered(serviceType, implementingTypes, lifeTimeFactory, serviceNameFormatter);
    }

    public IServiceRegistry RegisterPropertyDependency<TDependency>(Func<IServiceFactory, PropertyInfo, TDependency> factory)
    {
        return this.Container.RegisterPropertyDependency(factory);
    }

    public IServiceRegistry SetDefaultLifetime<T>() where T : ILifetime, new()
    {
        return this.Container.SetDefaultLifetime<T>();
    }
}
