using System.Diagnostics;

namespace Mini.Engine.ECS.Experimental;

public enum LifeCycle : byte
{
    Free,
    Created,
    New,
    Changed,
    Unchanged,
    Removed,
}

public struct ComponentLifeCycle
{
    public LifeCycle Current;
    public LifeCycle Next;

    public bool IsAlive => this.Current != LifeCycle.Free;
    public bool IsFree => this.Current == LifeCycle.Free;

    public void New()
    {
        this.Current = LifeCycle.Created;
        this.Next = LifeCycle.New;
    }

    public void Change()
    {
        this.Next = LifeCycle.Changed;
    }

    public void Remove()
    {
        this.Next = LifeCycle.Removed;
    }

    public void Advance()
    {
        this.Current = this.Next;
        if (this.Current == LifeCycle.Removed)
        {
            this.Next = LifeCycle.Free;
        }
        else
        {
            this.Next = LifeCycle.Unchanged;
        }
    }

    public override string ToString()
    {
        return $"{this.Current} -> {this.Next}";
    }
}

public interface IStructComponent
{
    public int ContainerIndex { get; set; }
}

public struct ComponentFilterEnumerable<T>
    where T : struct, IStructComponent
{
    private static readonly LifeCycle[] LivingLifeCycles = new[]
    {
        LifeCycle.Changed,
        LifeCycle.Created,
        LifeCycle.New,
        LifeCycle.Removed,
        LifeCycle.Unchanged,
    };

    private readonly T[] Pool;
    private readonly ComponentLifeCycle[] LifeCycles;
    private readonly LifeCycle[] Filters;
    private readonly int MaxIndex;
    private int index;

    public ComponentFilterEnumerable(T[] pool, ComponentLifeCycle[] lifeCycles, LifeCycle[] filters, int maxIndex)
    {
        this.Pool = pool;
        this.LifeCycles = lifeCycles;
        this.Filters = filters;
        this.MaxIndex = maxIndex;
        this.index = -1;
    }

    public static ComponentFilterEnumerable<T> All(T[] pool, ComponentLifeCycle[] lifeCycles, int maxIndex)
    {
        Debug.Assert(LivingLifeCycles.Length == Enum.GetValues(typeof(LifeCycle)).Length - 1);

        return new ComponentFilterEnumerable<T>(pool, lifeCycles, LivingLifeCycles, maxIndex);
    }

    public ComponentFilterEnumerable<T> GetEnumerator()
    {
        return this;
    }

    public ref T Current => ref this.Pool[this.index];

    public bool MoveNext()
    {
        this.index++;

        while (this.index <= this.MaxIndex)
        {
            var lifeCycle = this.LifeCycles[this.index].Current;
            for (var i = 0; i < this.Filters.Length; i++)
            {
                if (lifeCycle == this.Filters[i])
                {
                    return true;
                }
            }

            this.index++;
        }

        return false;
    }
}


public sealed class StructComponentContainer<T>
    where T : struct, IStructComponent
{
    private const int InitialBufferSize = 10;

    private int lowestUnusedSlot;
    private int highestUsedSlot;
    private T[] pool;
    private ComponentLifeCycle[] lifeCycles;

    public StructComponentContainer()
    {
        this.pool = new T[InitialBufferSize];
        this.lifeCycles = new ComponentLifeCycle[this.pool.Length];

        this.lowestUnusedSlot = 0;
        this.highestUsedSlot = -1;

        this.SetIndices(0);
    }

    public int Count { get; private set; }

    public float Fragmentation
    {
        get
        {
            if (this.Count == 0) { return 0.0f; }
            return 1.0f - (this.Count / (float)this.highestUsedSlot);
        }
    }

    public ref T Create()
    {
        var index = this.lowestUnusedSlot;

        if (this.Count < this.pool.Length)
        {
            this.lowestUnusedSlot = this.IndexOfFirstUnused(this.lowestUnusedSlot + 1);
            this.highestUsedSlot = Math.Max(this.highestUsedSlot, index);
        }
        else
        {
            this.Grow();
            this.highestUsedSlot = this.Count;
            this.lowestUnusedSlot = this.Count + 1;
        }

        this.lifeCycles[index].New();
        this.Count++;

        return ref this.pool[index];
    }

    public void AdvanceLifeCycles()
    {
        for (var i = 0; i < this.lifeCycles.Length; i++)
        {
            ref var lifeCycle = ref this.lifeCycles[i];
            if (lifeCycle.Current == LifeCycle.Removed)
            {
                this.Destroy(i);
                this.Count--;
            }

            lifeCycle.Advance();
        }
    }

    private void Destroy(int index)
    {
        (this.pool[index] as IDisposable)?.Dispose();

        if (index == this.highestUsedSlot)
        {
            this.highestUsedSlot = this.IndexOfLastUsed(index - 1);
        }

        this.lowestUnusedSlot = Math.Min(this.lowestUnusedSlot, index);
    }

    public void Defrag()
    {
        for (var i = this.highestUsedSlot; i > (this.Count - 1); i--)
        {
            if (this.lifeCycles[i].IsAlive)
            {
                this.pool[this.lowestUnusedSlot] = this.pool[i];
                this.pool[this.lowestUnusedSlot].ContainerIndex = this.lowestUnusedSlot;
                this.lifeCycles[this.lowestUnusedSlot] = this.lifeCycles[i];

                this.pool[i].ContainerIndex = i;
                this.lifeCycles[i].Current = LifeCycle.Free;
                this.lifeCycles[i].Next = LifeCycle.Free;

                this.lowestUnusedSlot = this.IndexOfFirstUnused(this.lowestUnusedSlot + 1);
            }
        }

        this.highestUsedSlot = this.Count - 1;

        if (this.pool.Length >= InitialBufferSize && this.pool.Length >= this.Count * 4)
        {
            this.Shrink(this.Count * 2);
        }
    }

    public ComponentFilterEnumerable<T> EnumerateAll()
    {
        return ComponentFilterEnumerable<T>.All(this.pool, this.lifeCycles, this.highestUsedSlot);
    }

    public unsafe ComponentFilterEnumerable<T> Enumerate(LifeCycle state)
    {
        // TODO: can we change componentfilterenumerable so that we don't have to new an array?
        return new ComponentFilterEnumerable<T>(this.pool, this.lifeCycles, new LifeCycle[] { state }, this.highestUsedSlot);
    }

    private int IndexOfFirstUnused(int minIndex)
    {
        if (minIndex < this.pool.Length)
        {

            for (var i = minIndex; i < this.pool.Length; i++)
            {
                if (this.lifeCycles[i].IsFree)
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
                if (this.lifeCycles[i].IsAlive)
                {
                    return i;
                }
            }
        }

        return -1;
    }

    private void SetIndices(int minIndex)
    {
        for (var i = minIndex; i < this.pool.Length; i++)
        {
            this.pool[i].ContainerIndex = i;
        }
    }

    private void Grow()
    {
        var length = this.pool.Length;

        Array.Resize(ref this.pool, this.pool.Length * 2);
        Array.Resize(ref this.lifeCycles, this.pool.Length);

        this.SetIndices(length);
    }

    private void Shrink(int reserve = 0)
    {
        var minSize = Math.Max(this.Count, reserve);
        Array.Resize(ref this.pool, minSize);
        Array.Resize(ref this.lifeCycles, this.pool.Length);
    }
}
