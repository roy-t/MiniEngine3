using System.ComponentModel;
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

    public ref T Create<T>(Entity entity)
        where T : struct, IComponent
    {
        var container = this.ContainerStore.GetContainer<T>();
        return ref container.Create(entity);
    }

    //public void Add<T>(T component)
    //    where T : Component
    //{
    //    this.ContainerStore.GetContainer<T>().Add(component);
    //}

    //public void Add<T, U>(T componentA, U componentB)
    //    where T : Component
    //    where U : Component
    //{
    //    this.Add(componentA);
    //    this.Add(componentB);
    //}

    //public void Add<T, U, V>(T componentA, U componentB, V componentC)
    //    where T : Component
    //    where U : Component
    //    where V : Component
    //{
    //    this.Add(componentA);
    //    this.Add(componentB);
    //    this.Add(componentC);
    //}

    //public void Add<T, U, V, W>(T componentA, U componentB, V componentC, W componentD)
    //    where T : Component
    //    where U : Component
    //    where V : Component
    //    where W : Component
    //{
    //    this.Add(componentA);
    //    this.Add(componentB);
    //    this.Add(componentC);
    //    this.Add(componentD);
    //}

    public ref T GetComponent<T>(Entity entity)
        where T : struct, IComponent
    {
        var store = this.ContainerStore.GetContainer<T>();
        return ref store[entity];
    }

    //public IReadOnlyList<Component> GetComponents(Entity entity)
    //{
    //    var components = new List<Component>();

    //    var containers = this.ContainerStore.GetAllContainers();
    //    for (var i = 0; i < containers.Count; i++)
    //    {
    //        var container = containers[i];
    //        if (container.Contains(entity))
    //        {
    //            components.Add(container.Get(entity));
    //        }
    //    }

    //    return components;
    //}

    //public IReadOnlyList<T> GetComponents<T>()
    //    where T : Component
    //{
    //    var components = new List<T>();

    //    var container = this.ContainerStore.GetContainer<T>();
    //    foreach (var component in container.GetAllItems())
    //    {
    //        components.Add(component);
    //    }

    //    return components;
    //}

    public void MarkForRemoval(Entity entity)
    {
        var containers = this.ContainerStore.GetAllContainers();
        for (var i = 0; i < containers.Count; i++)
        {
            var container = containers[i];
            if (container.Contains(entity))
            {
                container.Remove(entity);
            }
        }
    }
}
