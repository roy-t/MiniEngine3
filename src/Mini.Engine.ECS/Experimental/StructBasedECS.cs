namespace Mini.Engine.ECS.Experimental;

public enum LifeCycle
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
    private LifeCycle current;
    private LifeCycle next;

    public bool IsAlive => this.current != LifeCycle.Free;
    public bool IsFree => this.current == LifeCycle.Free;

    public void New()
    {
        this.current = LifeCycle.Created;
        this.next = LifeCycle.New;
    }

    public void Change()
    {
        this.next = LifeCycle.Changed;
    }

    public void Remove()
    {
        this.next = LifeCycle.Removed;
    }

    public void Next()
    {
        this.current = this.next;
        if (this.current == LifeCycle.Removed)
        {
            this.next = LifeCycle.Free;
        }
        else if (this.current == LifeCycle.Free)
        {
            this.next = LifeCycle.Free;
        }
        else
        {
            this.next = LifeCycle.Unchanged;
        }
    }

    public override string ToString()
    {
        return $"{this.current} -> {this.next}";
    }
}

public interface IStructComponent
{
    ComponentLifeCycle LifeCycle { get; }
}

public sealed class StructComponentContainer<T>
    where T : struct, IStructComponent
{
    private const int InitialBufferSize = 10;

    private int highestUsedSlot;
    private T[] pool;

    public StructComponentContainer()
    {
        this.pool = new T[InitialBufferSize];
        this.highestUsedSlot = 0;
    }

    public int Count { get; private set; }

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

        ref var component = ref this.pool[index];
        component.LifeCycle.New();

        return ref component;
    }

    public void Defrag()
    {
        var lowestFreeSlot = 0;
        for (var i = this.highestUsedSlot; i > (this.Count - 1); i++)
        {
            if (this.pool[i].LifeCycle.IsAlive)
            {
                lowestFreeSlot = this.IndexOfFirstUnused(lowestFreeSlot);
                this.pool[lowestFreeSlot] = this.pool[i];
            }
        }

        this.highestUsedSlot = this.Count - 1;

        if (this.pool.Length >= this.Count * 4)
        {
            this.Shrink(this.pool.Length * 2);
        }
    }

    private int IndexOfFirstUnused(int offset = 0)
    {
        for (var i = offset; i < this.pool.Length; i++)
        {
            if (this.pool[i].LifeCycle.IsFree)
            {
                return i;
            }
        }

        return -1;
    }

    private void Grow()
    {
        Array.Resize(ref this.pool, this.pool.Length * 2);
    }

    private void Shrink(int reserve = 0)
    {
        var minSize = Math.Max(this.Count, reserve);
        Array.Resize(ref this.pool, minSize);
    }    
}
