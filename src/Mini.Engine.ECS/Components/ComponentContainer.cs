using System;
using System.Collections.Generic;

namespace Mini.Engine.ECS.Components
{
    public interface IComponentContainer
    {
        Type ComponentType { get; }
        int Count { get; }
        AComponent Get(Entity entity);
        bool Contains(Entity entity);
        void Flush();
        void MarkForRemoval(Entity entity);
    }

    public interface IComponentContainer<T> : IComponentContainer
        where T : AComponent
    {
        void Add(T component);
        T this[Entity entity] { get; }
        IEnumerable<T> GetAllItems();
        IEnumerable<T> GetChangedItems();
        IEnumerable<T> GetNewItems();
        IEnumerable<T> GetRemovedItems();
        IEnumerable<T> GetUnchangedItems();
    }

    public sealed class ComponentContainer<T> : IComponentContainer<T>
        where T : AComponent
    {
        private readonly SortedComponentList<T> Items;

        public ComponentContainer()
        {
            this.Items = new SortedComponentList<T>();
        }

        public void Add(T component)
        {
            this.Items.Add(component);
        }

        public AComponent Get(Entity entity)
        {
            return this.Items[entity];
        }

        public T this[Entity entity] => this.Items[entity];

        public void MarkForRemoval(Entity entity)
        {
            this.Items[entity].ChangeState.Remove();
        }

        public int Count => this.Items.Count;

        public Type ComponentType => typeof(T);

        public bool Contains(Entity entity)
        {
            return this.Items.Contains(entity);
        }

        public void Flush()
        {
            for (var i = this.Items.Count - 1; i >= 0; i--)
            {
                var item = this.Items[i];
                if (item.ChangeState.CurrentState == LifetimeState.Removed)
                {
                    (item as IDisposable)?.Dispose();
                    this.Items.RemoveAt(i);
                }
                else
                {
                    item.ChangeState.Next();
                }
            }
        }

        public IEnumerable<T> GetAllItems()
        {
            return this.Items;
        }

        public IEnumerable<T> GetNewItems()
        {
            return this.FilterItems(LifetimeState.New);
        }

        public IEnumerable<T> GetChangedItems()
        {
            return this.FilterItems(LifetimeState.Changed);
        }

        public IEnumerable<T> GetUnchangedItems()
        {
            return this.FilterItems(LifetimeState.Unchanged);
        }

        public IEnumerable<T> GetRemovedItems()
        {
            return this.FilterItems(LifetimeState.Removed);
        }

        private IEnumerable<T> FilterItems(LifetimeState state)
        {
            for (var i = 0; i < this.Items.Count; i++)
            {
                var item = this.Items[i];
                if (item.ChangeState.CurrentState == state)
                {
                    yield return item;
                }
            }
        }
    }
}
