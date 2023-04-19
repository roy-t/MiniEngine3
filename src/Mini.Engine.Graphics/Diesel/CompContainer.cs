using System.Collections;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;

namespace Mini.Engine.Graphics.Diesel;

public sealed class CompContainer<T>
    where T : struct
{


    private const int InitialCapacity = 10;

    private readonly ComponentTracker Tracker;
    private readonly ComponentBit Bit;

    private readonly Pool<T> Pool;

    public CompContainer(ComponentTracker tracker)
    {
        this.Tracker = tracker;
        this.Bit = tracker.GetBit();
        this.Pool = new Pool<T>(InitialCapacity);
    }

    public bool Contains(Entity entity)
    {
        return this.Tracker.HasComponent(entity, this.Bit);
    }


    //public ILifetime<T> Create(Entity entity)
    //{

    //}
}

public struct Entry<T>
    where T : struct
{
    public T Component;
    public Entity Entity;
    public LifeCycle LifeCycle;
}

public sealed class Pool<T>
    where T : struct
{
    private readonly int MinimumCapacity;
    private readonly IndexTracker Tracker;
    private Entry<T>[] entries;

    public Pool(int initialCapacity)
    {
        this.MinimumCapacity = initialCapacity;
        this.Tracker = new IndexTracker(initialCapacity);
        this.entries = new Entry<T>[initialCapacity];
    }

    public int Count { get; private set; }
    public int Capacity => this.entries.Length;

    public ref Entry<T> this[int index]
    {
        get
        {
            if (index < this.Count)
            {
                return ref this.entries[index];
            }

            throw new IndexOutOfRangeException($"{index} >= {this.Count}");
        }
    }

    public ref Entry<T> this[Entity entity]
    {
        get
        {
            var index = this.Tracker.GetReference(entity);
            return ref this.entries[index];
        }
    }

    public ref Entry<T> CreateFor(Entity entity)
    {
        if (this.Count >= this.Capacity)
        {
            this.Resize(Math.Max(MinimumCapacity, this.Capacity * 2));
        }

        var index = this.Count;

        this.Tracker.InsertOrUpdate(entity, index);
        this.Count++;

        ref var entry = ref this.entries[index];
        entry.Entity = entity;
        entry.LifeCycle = LifeCycle.Init();

        return ref entry;
    }


    public void Destroy(int index)
    {
        var entity = this.entries[index].Entity;
        this.DestroyFor(entity);
    }

    public void DestroyFor(Entity entity)
    {
        var index = this.Tracker.Remove(entity);
        this.entries[index] = default;
        this.FillGap(index);

        this.Count--;
    }

    public void Resize(int newCapacity)
    {
        if (newCapacity <= this.Capacity)
        {
            throw new InvalidOperationException($"Cannot resize to {newCapacity}, which is less than or equal to the current capacity {this.Capacity}");
        }

        Array.Resize(ref this.entries, newCapacity);
        this.Tracker.Reserve(newCapacity);
    }

    public void Trim()
    {
        if (this.Count < this.Capacity)
        {
            this.Resize(this.Count);
        }
    }

    private void FillGap(int gapIndex)
    {
        var low = gapIndex;
        var high = this.Count - 1;

        if (low < high)
        {
            this.entries[low] = this.entries[high];
            this.entries[high] = default;

            this.Tracker.InsertOrUpdate(this.entries[low].Entity, low);
        }
    }
}
