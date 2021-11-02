using System.Collections.Generic;
using Mini.Engine.Configuration;

namespace Mini.Engine.ECS.Components
{
    [Service]
    public sealed class ComponentAdministrator
    {
        private readonly ContainerStore ContainerStore;

        public ComponentAdministrator(ContainerStore containerStore)
        {
            this.ContainerStore = containerStore;
        }

        public void Add<T>(ref T component)
            where T : struct, IComponent
        {
            this.ContainerStore.GetContainer<T>().Add(ref component);
        }

        public void Add<T, U>(ref T componentA, ref U componentB)
            where T : struct, IComponent
            where U : struct, IComponent
        {
            this.ContainerStore.GetContainer<T>().Add(ref componentA);
            this.ContainerStore.GetContainer<U>().Add(ref componentB);
        }

        public void Add<T, U, V>(ref T componentA, ref U componentB, ref V componentC)
            where T : struct, IComponent
            where U : struct, IComponent
            where V : struct, IComponent
        {
            this.ContainerStore.GetContainer<T>().Add(ref componentA);
            this.ContainerStore.GetContainer<U>().Add(ref componentB);
            this.ContainerStore.GetContainer<V>().Add(ref componentC);
        }

        public void Add<T, U, V, W>(ref T componentA, ref U componentB, ref V componentC, ref W componentD)
            where T : struct, IComponent
            where U : struct, IComponent
            where V : struct, IComponent
            where W : struct, IComponent
        {
            this.ContainerStore.GetContainer<T>().Add(ref componentA);
            this.ContainerStore.GetContainer<U>().Add(ref componentB);
            this.ContainerStore.GetContainer<V>().Add(ref componentC);
            this.ContainerStore.GetContainer<W>().Add(ref componentD);
        }

        public T GetComponent<T>(Entity entity)
            where T : struct, IComponent
        {
            var store = this.ContainerStore.GetContainer<T>();
            var component = store[entity];

            return component;
        }

        public IReadOnlyList<T> GetComponents<T>()
            where T : struct, IComponent
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
                    container.Remove(entity);
                }
            }
        }
    }
}
