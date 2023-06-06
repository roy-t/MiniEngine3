namespace Mini.Engine.Core.Lifetime;
internal sealed class StackPool
{
    private const int InitialCapacity = 100;

    // TODO: use StructPool and ReferencePool here?
    private IDisposable?[] pool;
    private int[] versions;

    private int lowestUnusedSlot = 0;
    private int highestUsedSlot = -1;

    private int count;

    public StackPool()
    {
        this.versions = new int[InitialCapacity];
        this.pool = new IDisposable?[InitialCapacity];
    }

    public IDisposable this[ILifetime resource]
    {
        get
        {
            if (this.IsValid(resource))
            {
                return this.pool[resource.Id]!;
            }
            throw new Exception($"The resource pointed to by {resource} no longer exists");
        }
    }

    public bool IsValid(ILifetime resource)
    {
        return this.versions[resource.Id] == resource.Version;
    }

    public ILifetime<T> Add<T>(T resource, int version)
        where T : IDisposable
    {
        this.EnsureCapacity(this.count + 1);

        var index = this.lowestUnusedSlot;
        this.lowestUnusedSlot = this.IndexOfFirstUnused(this.lowestUnusedSlot + 1);
        this.highestUsedSlot = Math.Max(this.highestUsedSlot, index);

        this.pool[index] = resource;
        this.versions[index] = version;

        this.count += 1;
        return new StandardLifetime<T>(index, version);
    }

    public void DisposeAll(int version)
    {
        for (var i = 0; i < this.versions.Length; i++)
        {
            if (this.versions[i] == version)
            {
                this.Remove(i);
            }
        }
    }

    private void Remove(int index)
    {
        this.pool[index]?.Dispose();
        this.pool[index] = null;
        this.versions[index] = 0;

        if (index == this.highestUsedSlot)
        {
            this.highestUsedSlot = this.IndexOfLastUsed(index - 1);
        }

        this.lowestUnusedSlot = Math.Min(this.lowestUnusedSlot, index);

        this.count -= 1;
    }

    private void EnsureCapacity(int capacity)
    {
        if (capacity >= this.pool.Length)
        {
            var newCapacity = Math.Max(capacity, this.pool.Length * 2);

            Array.Resize(ref this.versions, newCapacity);
            Array.Resize(ref this.pool, newCapacity);
        }
    }

    private int IndexOfFirstUnused(int minIndex)
    {
        if (minIndex < this.pool.Length)
        {
            for (var i = minIndex; i < this.pool.Length; i++)
            {
                if (this.versions[i] <= 0)
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
                if (this.versions[i] > 0)
                {
                    return i;
                }
            }
        }

        return -1;
    }
}
