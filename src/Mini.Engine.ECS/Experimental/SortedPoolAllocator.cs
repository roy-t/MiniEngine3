using System.Collections;

namespace Mini.Engine.ECS.Experimental;
public sealed class SortedPoolAllocator<T>
    where T : struct, IComponent
{
    private readonly BitArray Occupancy;
    private T[] pool;
    private int[] lookup;

    public SortedPoolAllocator(int capacity)
    {
        this.pool = new T[capacity];
        this.lookup = new int[capacity];
        this.Occupancy = new BitArray(capacity);
    }

    public int Count { get; private set; }
    public int Capacity => this.pool.Length;

    public ref T this[int index] => ref this.pool[index];

    public bool IsOccupied(int index)
    {
        return this.Occupancy[index];
    }

    public ref T CreateFor(Entity entity)
    {
        if (this.Capacity < this.Count - 1)
        {
            this.Reserve(this.Capacity * 2);
        }

        var index = Array.BinarySearch(this.lookup, entity.Id);
        if (index >= 0)
        {
            throw new Exception($"Component for {entity} already exists");
        }

        index = ~index;
        this.Insert(index);

        return ref this.pool[index];
    }

    private void Insert(int index)
    {
        Array.Copy(this.pool, index, this.pool, index + 1, this.Count - index);
        // TODO: also d this for occupancy, lookup!
        this.Count++;
        this.Occupancy[index] = true;
    }

    public void Destroy(int index)
    {
        if (this.Occupancy[index])
        {
            this.pool[index].Destroy();
            Array.Copy(this.pool, index + 1, this.pool, index, this.Count - index);

            // TODO: also d this for occupancy, lookup!
        }
    }

    public void Reserve(int newCapacity)
    {
        if (newCapacity < this.pool.Length)
        {
            throw new InvalidOperationException($"Cannot grow to {newCapacity}, which is less than the current capacity {this.Capacity}");
        }

        Array.Resize(ref this.pool, newCapacity);
        this.Occupancy.Length = newCapacity;
    }

    public void Trim()
    {
        this.Trim(this.Count);
    }

    public void Trim(int newCapacity)
    {
        if (newCapacity < (this.Count))
        {
            throw new InvalidOperationException($"Cannot trim to {newCapacity}, which is less than the current count of elements {this.Count}");
        }

        newCapacity = Math.Max(this.Count, newCapacity);

        Array.Resize(ref this.pool, newCapacity);
        this.Occupancy.Length = newCapacity;
    }
}
