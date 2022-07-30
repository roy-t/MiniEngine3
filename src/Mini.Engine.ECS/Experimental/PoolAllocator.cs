﻿using System.Collections;

namespace Mini.Engine.ECS.Experimental;


public interface IComponent
{
    public Entity Entity { get; }
    public void Destroy();
}

public sealed class PoolAllocator<T>
    where T : struct, IComponent
{
    private readonly BitArray Occupancy;
    private readonly IndexTracker Tracker;
    private T[] pool;

    public PoolAllocator(int capacity)
    {
        this.Occupancy = new BitArray(capacity);
        this.Tracker = new IndexTracker(capacity);
        this.pool = new T[capacity];

        this.LowestUnusedSlot = 0;
        this.HighestUsedSlot = -1;
    }

    public int LowestUnusedSlot { get; private set; }
    public int HighestUsedSlot { get; private set; }

    public int Count { get; private set; }
    public int Capacity => this.pool.Length;

    public float Fragmentation
    {
        get
        {
            if (this.Count == 0 || this.HighestUsedSlot == 0)
            {
                return 0.0f;
            }
            return 1.0f - (this.Count / (this.HighestUsedSlot + 1.0f));
        }
    }

    public ref T this[int index] => ref this.pool[index];

    public bool IsOccupied(int index)
    {
        return this.Occupancy[index];
    }

    public ref T CreateFor(Entity entity)
    {
        var index = this.LowestUnusedSlot;

        if (this.Count < this.Capacity)
        {
            this.LowestUnusedSlot = this.IndexOfFirstUnused(this.LowestUnusedSlot + 1);
            this.HighestUsedSlot = Math.Max(this.HighestUsedSlot, index);
        }
        else
        {
            this.Reserve(this.Capacity * 2);
            this.HighestUsedSlot = this.Count;
            this.LowestUnusedSlot = this.Count + 1;
        }

        this.Occupancy[index] = true;
        this.Tracker.InsertOrUpdate(entity, index);
        this.Count++;

        return ref this.pool[index];
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
        this.Count--;

        if (index == this.HighestUsedSlot)
        {
            this.HighestUsedSlot = this.IndexOfLastUsed(index - 1);
        }

        this.LowestUnusedSlot = Math.Min(this.LowestUnusedSlot, index);

        this.SwapHighestUsedWithLowestUnused();
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
        this.Trim(this.HighestUsedSlot + 1);
    }

    public void Trim(int newCapacity)
    {
        if (newCapacity < (this.HighestUsedSlot + 1))
        {
            throw new InvalidOperationException($"Cannot trim to {newCapacity}, which is less than the currently highest used slot {this.HighestUsedSlot}");
        }

        newCapacity = Math.Max(this.Count, newCapacity);

        Array.Resize(ref this.pool, newCapacity);
        this.Occupancy.Length = newCapacity;
    }

    private void SwapHighestUsedWithLowestUnused()
    {
        var entity = this.pool[this.HighestUsedSlot].Entity;

        this.pool[this.LowestUnusedSlot] = this.pool[this.HighestUsedSlot];

        this.Occupancy[this.HighestUsedSlot] = false;
        this.Occupancy[this.LowestUnusedSlot] = true;

        this.Tracker.InsertOrUpdate(entity, this.LowestUnusedSlot);

        this.LowestUnusedSlot = this.IndexOfFirstUnused(this.LowestUnusedSlot + 1);
        this.HighestUsedSlot = this.IndexOfLastUsed(this.HighestUsedSlot - 1);
    }

    private int IndexOfFirstUnused(int minIndex)
    {
        if (minIndex < this.pool.Length)
        {
            for (var i = minIndex; i < this.pool.Length; i++)
            {
                if (!this.Occupancy[i])
                {
                    return i;
                }
            }
        }

        return this.pool.Length;
    }

    private int IndexOfLastUsed(int maxIndex)
    {
        if (this.pool.Length > maxIndex)
        {
            for (var i = maxIndex; i >= 0; i--)
            {
                if (this.Occupancy[i])
                {
                    return i;
                }
            }
        }

        return -1;
    }
}
