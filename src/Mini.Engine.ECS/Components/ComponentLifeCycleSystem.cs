using Mini.Engine.Configuration;

namespace Mini.Engine.ECS.Components;

[Service]
public sealed class ComponentLifeCycleSystem
{
    private readonly ContainerStore ContainerStore;

    public ComponentLifeCycleSystem(ContainerStore containerStore)
    {
        this.ContainerStore = containerStore;
    }
  
    public void Run()
    {
        var containers = this.ContainerStore.GetAllContainers();
        for (var i = 0; i < containers.Count; i++)
        {
            containers[i].UpdateLifeCycles();
        }
    }
}
