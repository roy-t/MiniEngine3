using Mini.Engine.Configuration;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.ECS.Systems;

namespace Mini.Engine.ECS.Components
{
    [System]
    public partial class ComponentFlushSystem : ISystem
    {
        private readonly ContainerStore ContainerStore;

        public ComponentFlushSystem(ContainerStore containerStore) => this.ContainerStore = containerStore;

        public void OnSet()
        {
        }

        [Process]
        public void Process()
        {
            var containers = this.ContainerStore.GetAllContainers();
            for (var i = 0; i < containers.Count; i++)
            {
                containers[i].Flush();
            }
        }
    }
}
