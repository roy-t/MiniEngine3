using System;
using LightInject;

namespace Mini.Engine.Configuration;

public sealed class Services
{
    private readonly ServiceContainer Container;

    public Services(ServiceContainer container)
    {
        this.Container = container;
    }

    public T Resolve<T>()
    {
        return this.Container.GetInstance<T>();
    }

    public bool TryResolve<T>(out T instance)
    {
        instance = this.Container.TryGetInstance<T>();
        return instance != null;
    }

    public T Resolve<T>(Type subType)
    {
        return (T)this.Container.GetInstance(subType);
    }

    public void Register<T>(T instance)
    {
        this.Container.RegisterInstance(instance);
    }

    public void RegisterAs<TSource, TTarget>(TSource instance)
        where TSource : TTarget
    {
        this.Container.RegisterInstance<TTarget>(instance);
    }
}
