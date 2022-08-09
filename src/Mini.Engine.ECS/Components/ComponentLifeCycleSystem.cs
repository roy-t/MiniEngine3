using Mini.Engine.Configuration;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.ECS.Systems;

namespace Mini.Engine.ECS.Components;

[System]
public sealed partial class ComponentLifeCycleSystem : ISystem
{
    private readonly ContainerStore ContainerStore;

    public ComponentLifeCycleSystem(ContainerStore containerStore)
    {
        this.ContainerStore = containerStore;
    }

    public void OnSet()
    {
    }

    public void OnUnSet()
    {

    }

    [Process]
    public void Process()
    {
        var containers = this.ContainerStore.GetAllContainers();
        for (var i = 0; i < containers.Count; i++)
        {
            containers[i].UpdateLifeCycles();
        }
    }
}
