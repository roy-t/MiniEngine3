using System;
using System.Collections;
using System.Collections.Generic;

namespace Mini.Engine.ECS.Components
{
    public sealed class SortedComponentList<T> : IEnumerable<T>
        where T : struct, IComponent
    {
        private const int DefaultCapacity = 4;
        private const int GrowthFactor = 2;

        private T[] components;

        public SortedComponentList(int capacity = DefaultCapacity)
        {
            this.components = new T[capacity];
        }

        public int Count { get; private set; }

        public ref T this[int i] => ref this.components[i];

        public ref T this[Entity entity]
        {
            get
            {
                var index = this.BinarySearch(entity);
                return ref this.components[index];
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new ArraySegment<T>(this.components, 0, this.Count).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new ArraySegment<T>(this.components, 0, this.Count).GetEnumerator();
        }

        public void Remove(Entity entity)
        {
            var index = this.BinarySearch(entity);
            this.RemoveAt(index);
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= this.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            this.Count--;
            if (index < this.Count)
            {
                Array.Copy(this.components, index + 1, this.components, index, this.Count - index);
            }

            this.components[this.Count] = default;
        }

        public void Add(ref T component)
        {
            var index = this.BinarySearch(component.Entity);
            if (index >= 0)
            {
                throw new ArgumentException($"Adding component with duplicate key {component.Entity.Id}");
            }

            this.Insert(~index, ref component);
        }

        public bool Contains(Entity entity)
        {
            return this.BinarySearch(entity) >= 0;
        }

        private void Insert(int index, ref T component)
        {
            if (this.Count == this.components.Length)
            {
                this.EnsureCapacity(this.Count + 1);
            }

            if (index < this.Count)
            {
                Array.Copy(this.components, index, this.components, index + 1, this.Count - index);
            }

            this.components[index] = component;
            this.Count++;
        }

        private void EnsureCapacity(int capacity)
        {
            capacity = Math.Max(capacity, this.components.Length * GrowthFactor);
            Array.Resize(ref this.components, capacity);
        }

        private int BinarySearch(Entity entity)
        {
            var low = 0;
            var high = this.Count - 1;
            while (low <= high)
            {
                var mid = low + ((high - low) >> 1);
                var order = this.components[mid].Entity.Id.CompareTo(entity.Id);
                if (order == 0)
                {
                    return mid;
                }
                if (order < 0)
                {
                    low = mid + 1;
                }
                else
                {
                    high = mid - 1;
                }
            }

            return ~low;
        }
    }
}
