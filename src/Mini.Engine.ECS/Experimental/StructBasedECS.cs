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
    
}

public struct ComponentFilterEnumerable<T>
    where T : struct, IStructComponent
{
    private readonly T[] Pool;
    private readonly ComponentLifeCycle[] LifeCycles;
    private readonly LifeCycle Filter;
    private readonly int MaxIndex;
    private int index;
    
    public ComponentFilterEnumerable(T[] pool, ComponentLifeCycle[] lifeCycles, LifeCycle filter, int maxIndex)
    {
        this.Pool = pool;
        this.LifeCycles = lifeCycles;
        this.Filter = filter;
        this.MaxIndex = maxIndex;
        this.index = -1;
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
            if (this.LifeCycles[this.index].Current == this.Filter)
            {
                return true;
            }

            this.index++;
        }

        return false;
    }
}

public struct AllComponentsEnumerable<T>
    where T : struct, IStructComponent
{
    private readonly T[] Pool;
    private readonly ComponentLifeCycle[] LifeCycles;
    private readonly int MaxIndex;
    private int index;

    public AllComponentsEnumerable(T[] pool, ComponentLifeCycle[] lifeCycles, int maxIndex)
    {
        this.Pool = pool;
        this.LifeCycles = lifeCycles;
        this.index = -1;
        this.MaxIndex = maxIndex;
    }


    public AllComponentsEnumerable<T> GetEnumerator()
    {
        return this;
    }

    public ref T Current => ref this.Pool[this.index];

    public bool MoveNext()
    {
        this.index++;

        while (this.index <= this.MaxIndex)
        {
            if (this.LifeCycles[this.index].Current != LifeCycle.Free)
            {
                return true;
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

    private int highestUsedSlot;
    private T[] pool;
    private ComponentLifeCycle[] lifeCycles;

    public StructComponentContainer()
    {
        this.pool = new T[InitialBufferSize];
        this.lifeCycles = new ComponentLifeCycle[this.pool.Length];

        this.highestUsedSlot = -1;
    }

    public int Count { get; private set; }

    public float Fragmentation
    {
        get
        {
            if(this.Count == 0) { return 0.0f; }
            return 1.0f - (this.highestUsedSlot / (float)this.Count);
        }
    }   

    public ref T Create()
    {
        if (this.Count == this.pool.Length)
        {
            this.Grow();
            this.highestUsedSlot = this.Count++;
            return ref this.pool[this.highestUsedSlot];
        }

        var index = this.IndexOfFirstUnused();
        this.highestUsedSlot = Math.Max(this.highestUsedSlot, index);
                
        this.lifeCycles[index].New();
        return ref this.pool[index];
    }

    public void AdvanceLifeCycles()
    {
        for (var i = 0; i < this.lifeCycles.Length; i++)
        {
            ref var lifeCycle = ref this.lifeCycles[i];
            if (lifeCycle.Current == LifeCycle.Removed)
            {
                (this.pool[i] as IDisposable)?.Dispose();

                if (i == this.highestUsedSlot)
                {
                    this.highestUsedSlot = this.IndexOfLastUsed(i - 1);
                }
            }

            lifeCycle.Advance();
        }
    }

    public void Defrag()
    {
        var lowestFreeSlot = 0;
        for (var i = this.highestUsedSlot; i > (this.Count - 1); i++)
        {
            if (this.lifeCycles[i].IsAlive)
            {
                lowestFreeSlot = this.IndexOfFirstUnused(lowestFreeSlot);

                this.pool[lowestFreeSlot] = this.pool[i];
                this.lifeCycles[lowestFreeSlot] = this.lifeCycles[i];

                this.lifeCycles[i].Current = LifeCycle.Free;
                this.lifeCycles[i].Next = LifeCycle.Free;
            }
        }

        this.highestUsedSlot = this.Count - 1;

        if (this.pool.Length >= this.Count * 4)
        {
            this.Shrink(this.pool.Length * 2);
        }
    }

    public AllComponentsEnumerable<T> EnumerateAll()
    {
        return new AllComponentsEnumerable<T>(this.pool, this.lifeCycles, this.highestUsedSlot);
    }

    public ComponentFilterEnumerable<T> Enumerate(LifeCycle state)
    {
        return new ComponentFilterEnumerable<T>(this.pool, this.lifeCycles, state, this.highestUsedSlot);
    }

    private int IndexOfFirstUnused(int minIndex = 0)
    {
        for (var i = minIndex; i < this.pool.Length; i++)
        {
            if (this.lifeCycles[i].IsFree)
            {
                return i;
            }
        }

        return -1;
    }

    private int IndexOfLastUsed(int maxIndex)
    {
        if (this.Count == 0)
        {
            return -1;
        }

        for (var i = maxIndex; i >= 0; i--)
        {
            if (this.lifeCycles[i].IsAlive)
            {
                return i;
            }
        }

        throw new Exception($"Unreachable exception, could not find index of last used maxIndex: {maxIndex}, count {this.Count}");
    }

    private void Grow()
    {
        Array.Resize(ref this.pool, this.pool.Length * 2);
        Array.Resize(ref this.lifeCycles, this.pool.Length);
    }

    private void Shrink(int reserve = 0)
    {
        var minSize = Math.Max(this.Count, reserve);
        Array.Resize(ref this.pool, minSize);
        Array.Resize(ref this.lifeCycles, this.pool.Length);
    }
}
