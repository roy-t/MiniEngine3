using System;
using System.Collections.Generic;

namespace Mini.Engine.ECS.Components
{
    public interface IComponent
    {
        public Entity Entity { get; }
        public ComponentChangeState ChangeState { get; }
    }

    public sealed class LookupArray<T>
        where T : struct, IComponent
    {
        private const int MinimumGrowth = 10;
        private const int GrowthFactor = 2;

        private T[] items;

        public LookupArray(int capacity)
        {
            this.items = new T[capacity];
        }

        public int Length { get; private set; }

        public T this[int index] => this.items[index];
        public T this[Entity entity] => this.Find(entity);

        public void Add(T item)
        {
            this.EnsureCapacity();

            var index = this.FindIndex(item.Entity.Id);
            if (index < this.Length)
            {
                Buffer.BlockCopy(this.items, index, this.items, index + 1, this.Length - index);
                this.items[index] = item;
            }
            else
            {
                this.items[index] = item;
            }

            this.Length++;
        }

        public void Remove(Entity entity)
        {
            this.Remove(this.FindIndex(entity.Id));
        }

        public void Remove(int index)
        {
            if (index >= 0 && index < this.Length)
            {
                if (index != this.Length - 1)
                {
                    Buffer.BlockCopy(this.items, index + 1, this.items, index, this.Length - index);
                }

                this.Length--;
            }
            else
            {
                throw new IndexOutOfRangeException($"index {index} is out of range [{0}..{this.Length})");
            }
        }

        public void Flush()
        {
            for (var i = this.Length - 1; i >= 0; i--)
            {
                if (this.items[i].ChangeState.CurrentState == LifetimeState.Removed)
                {
                    (this.items[i] as IDisposable)?.Dispose();
                    this.Remove(i);
                }
                else
                {
                    this.items[i].ChangeState.Next();
                }
            }
        }

        public IEnumerable<T> GetNewItems()
        {
            return this.GetItems(LifetimeState.New);
        }

        public IEnumerable<T> GetChangedItems()
        {
            return this.GetItems(LifetimeState.Changed);
        }

        public IEnumerable<T> GetUnchangedItems()
        {
            return this.GetItems(LifetimeState.Unchanged);
        }

        public IEnumerable<T> GetRemovedItems()
        {
            return this.GetItems(LifetimeState.Removed);
        }

        private IEnumerable<T> GetItems(LifetimeState state)
        {
            for (var i = 0; i < this.Length; i++)
            {
                if (this.items[i].ChangeState.CurrentState == state)
                {
                    yield return this.items[i];
                }
            }
        }

        private T Find(Entity entity)
        {
            var index = this.FindIndex(entity.Id);
            if (index >= 0 && index < this.Length)
            {
                return this.items[index];
            }

            throw new KeyNotFoundException($"Could not find item for key {entity.Id}");
        }

        private int FindIndex(int id)
        {
            if (this.Length == 0)
            {
                return 0;
            }

            var min = 0;
            var max = this.Length - 1;

            while (min <= max)
            {
                var mid = (min + max) / 2;
                var currentId = this.items[mid].Entity.Id;
                if (currentId < id)
                {
                    min = mid + 1;
                }

                if (currentId > id)
                {
                    max = mid - 1;
                }

                if (currentId == id)
                {
                    return mid;
                }
            }

            return min;
        }

        private void EnsureCapacity()
        {
            var required = this.Length + 1;
            var capacity = this.items.Length;
            if (required > capacity)
            {
                Array.Resize(ref this.items, Math.Max(capacity + MinimumGrowth, capacity * GrowthFactor));
            }
        }
    }
}
