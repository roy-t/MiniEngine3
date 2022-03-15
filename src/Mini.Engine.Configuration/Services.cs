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

    public object Resolve(Type type)
    {
        return this.Container.GetInstance(type);
    }

    public T Resolve<T>()
    {
        return this.Container.GetInstance<T>();
    }

    public T Resolve<T>(Type subType)
    {
        return (T)this.Container.GetInstance(subType);
    }

    public bool TryResolve<T>(out T instance)
    {
        instance = this.Container.TryGetInstance<T>();
        return instance != null;
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
