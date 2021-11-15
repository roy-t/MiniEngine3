using System;
using System.Collections.Generic;
using System.Linq;
using Mini.Engine.Configuration;

namespace Mini.Engine.ECS.Components;

[Service]
public sealed class ContainerStore
{
    private readonly IReadOnlyList<IComponentContainer> Containers;
    private readonly Dictionary<Type, IComponentContainer> ContainersByType;

    public ContainerStore(IEnumerable<IComponentContainer> containers)
    {
        this.Containers = containers.Distinct().ToList();
        this.ContainersByType = this.Containers.ToDictionary(x => x.ComponentType);
    }

    public IReadOnlyList<IComponentContainer> GetAllContainers()
    {
        return this.Containers;
    }

    public IComponentContainer<T> GetContainer<T>()
        where T : AComponent
    {
        var key = typeof(T);
        return (IComponentContainer<T>)this.ContainersByType[key];
    }
}
