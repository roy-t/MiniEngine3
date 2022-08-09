using System.Collections;

namespace Mini.Engine.ECS.Experimental;

public sealed class PoolAllocator<T>
    where T : struct, IComponent
{
    private const int MinimumCapacity = 10;

    private readonly BitArray Occupancy;
    private readonly IndexTracker Tracker;
    private T[] pool;

    public PoolAllocator(int capacity)
    {
        this.Occupancy = new BitArray(capacity);
        this.Tracker = new IndexTracker(capacity);
        this.pool = new T[capacity];
    }

    public int Count { get; private set; }
    public int Capacity => this.pool.Length;

    public ref T this[int index]
    {
        get
        {
            if (index < this.Count)
            {
                return ref this.pool[index];
            }

            throw new IndexOutOfRangeException($"{index} >= {this.Count}");
        }
    }

    public ref T this[Entity entity]
    {
        get
        {
            var index = this.Tracker.GetReference(entity);
            return ref this.pool[index];            
        }
    }

    public ref T CreateFor(Entity entity)
    {
        if (this.Count >= this.Capacity)
        {
            this.Reserve(Math.Max(MinimumCapacity, this.Capacity * 2));
        }

        var index = this.Count;

        this.Occupancy[index] = true;
        this.Tracker.InsertOrUpdate(entity, index);
        this.Count++;


        ref var component = ref this.pool[index];
        component.Entity = entity;
        component.LifeCycle = LifeCycle.Init();

        return ref component;
    }

    public void Destroy(int index)
    {
        var entity = this.pool[index].Entity;
        this.DestroyFor(entity);
    }

    public void DestroyFor(Entity entity)
    {
        var index = this.Tracker.Remove(entity);

        this.pool[index].Destroy();
        this.Occupancy[index] = false;

        this.FillGap(index);

        this.Count--;
    }

    public void Reserve(int newCapacity)
    {
        if (newCapacity < this.pool.Length)
        {
            throw new InvalidOperationException($"Cannot grow to {newCapacity}, which is less than the current capacity {this.Capacity}");
        }

        Array.Resize(ref this.pool, newCapacity);
        this.Occupancy.Length = newCapacity;
        this.Tracker.Reserve(newCapacity);
    }

    public void Trim()
    {
        this.Trim(this.Count);
    }

    public void Trim(int newCapacity)
    {
        if (newCapacity < this.Count)
        {
            throw new InvalidOperationException($"Cannot trim to {newCapacity}, which is less than the currently numer of items {this.Count}");
        }

        Array.Resize(ref this.pool, newCapacity);
        this.Occupancy.Length = newCapacity;
    }

    private void FillGap(int gapIndex)
    {
        var low = gapIndex;
        var high = this.Count - 1;

        if (low < high)
        {
            this.pool[low] = this.pool[high];
            this.pool[high] = default;
            this.Occupancy[high] = false;
            this.Occupancy[low] = true;

            this.Tracker.InsertOrUpdate(this.pool[low].Entity, low);
        }
    }
}
