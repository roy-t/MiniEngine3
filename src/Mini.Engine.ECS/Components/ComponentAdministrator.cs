using Mini.Engine.Configuration;

namespace Mini.Engine.ECS.Components;

[Service]
public sealed class ComponentAdministrator
{
    // TODO: class assumes single threaded component creation/deletion

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

    public bool HasComponent<T>(Entity entity)
        where T : struct, IComponent
    {
        var store = this.ContainerStore.GetContainer<T>();
        return entity.HasComponent(store);
    }
 
    public ref Component<T> GetComponent<T>(Entity entity)
        where T : struct, IComponent
    {
        var store = this.ContainerStore.GetContainer<T>();
        return ref store[entity];
    }   

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
