using System.Collections.Generic;
using Mini.Engine.Configuration;

namespace Mini.Engine.ECS.Components;

[Service]
public sealed class ComponentAdministrator
{
    private readonly ContainerStore ContainerStore;

    public ComponentAdministrator(ContainerStore containerStore)
    {
        this.ContainerStore = containerStore;
    }

    public void Add<T>(T component)
        where T : Component
    {
        this.ContainerStore.GetContainer<T>().Add(component);
    }

    public void Add<T, U>(T componentA, U componentB)
        where T : Component
        where U : Component
    {
        this.ContainerStore.GetContainer<T>().Add(componentA);
        this.ContainerStore.GetContainer<U>().Add(componentB);
    }

    public void Add<T, U, V>(T componentA, U componentB, V componentC)
        where T : Component
        where U : Component
        where V : Component
    {
        this.ContainerStore.GetContainer<T>().Add(componentA);
        this.ContainerStore.GetContainer<U>().Add(componentB);
        this.ContainerStore.GetContainer<V>().Add(componentC);
    }

    public void Add<T, U, V, W>(T componentA, U componentB, V componentC, W componentD)
        where T : Component
        where U : Component
        where V : Component
        where W : Component
    {
        this.ContainerStore.GetContainer<T>().Add(componentA);
        this.ContainerStore.GetContainer<U>().Add(componentB);
        this.ContainerStore.GetContainer<V>().Add(componentC);
        this.ContainerStore.GetContainer<W>().Add(componentD);
    }

    public T GetComponent<T>(Entity entity)
        where T : Component
    {
        var store = this.ContainerStore.GetContainer<T>();
        var component = store[entity];

        return component;
    }

    public IReadOnlyList<Component> GetComponents(Entity entity)
    {
        var components = new List<Component>();

        var containers = this.ContainerStore.GetAllContainers();
        for (var i = 0; i < containers.Count; i++)
        {
            var container = containers[i];
            if (container.Contains(entity))
            {
                components.Add(container.Get(entity));
            }
        }

        return components;
    }

    public IReadOnlyList<T> GetComponents<T>()
        where T : Component
    {
        var components = new List<T>();

        var container = this.ContainerStore.GetContainer<T>();
        foreach (var component in container.GetAllItems())
        {
            components.Add(component);
        }

        return components;
    }

    public void MarkForRemoval(Entity entity)
    {
        var containers = this.ContainerStore.GetAllContainers();
        for (var i = 0; i < containers.Count; i++)
        {
            var container = containers[i];
            if (container.Contains(entity))
            {
                container.MarkForRemoval(entity);
            }
        }
    }
}
