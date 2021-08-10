using Mini.Engine.Configuration;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.ECS.Systems;

namespace Mini.Engine.ECS.Components
{
    [System]
    public partial class ComponentFlushSystem : ISystem
    {
        private readonly ContainerStore ContainerStore;

        public ComponentFlushSystem(ContainerStore containerStore)
        {
            this.ContainerStore = containerStore;
        }

        public void OnSet()
        {
        }

        [Process(Query = ProcessQuery.Changed)]
        public void Changed(AComponent component)
        {

        }


        [Process]
        public void None()
        {

        }

        [Process(Query = ProcessQuery.All)]
        public void Process(AComponent component, AComponent b)
        {
            var containers = this.ContainerStore.GetAllContainers();
            for (var i = 0; i < containers.Count; i++)
            {
                containers[i].Flush();
            }
        }
    }
}
