namespace Mini.Engine.DirectX;

internal sealed class ResourcePool
{
    private const int InitialCapacity = 100;

    private int[] versions;
    private IDeviceResource?[] pool;

    private int lowestUnusedSlot = 0;
    private int highestUsedSlot = -1;

    private int count;

    public ResourcePool()
    {
        this.versions = new int[InitialCapacity];
        this.pool = new IDeviceResource?[InitialCapacity];
    }

    public IDeviceResource this[IResource resource]
    {
        get
        {
            if (this.versions[resource.Id] != resource.Version)
            {
                throw new Exception($"The resource pointed to by {resource} no longer exists");
            }

            return this.pool[resource.Id]!;
        }
    }

    public Resource<T> Add<T>(T resource)
        where T : IDeviceResource
    {
        this.EnsureCapacity(this.count + 1);

        var index = this.lowestUnusedSlot;
        this.lowestUnusedSlot = this.IndexOfFirstUnused(this.lowestUnusedSlot + 1);
        this.highestUsedSlot = Math.Max(this.highestUsedSlot, index);

        this.pool[index] = resource;

        var version = Math.Abs(this.versions[index]) + 1;
        this.versions[index] = version;

        this.count += 1;
        return new Resource<T>(index, version);
    }

    public void Remove(IResource resource, out IDeviceResource deviceResource)
    {
        deviceResource = this[resource];

        this.pool[resource.Id] = null;
        this.versions[resource.Id] = -this.versions[resource.Id];
        
        if (resource.Id == this.highestUsedSlot)
        {
            this.highestUsedSlot = this.IndexOfLastUsed(resource.Id - 1);
        }

        this.lowestUnusedSlot = Math.Min(this.lowestUnusedSlot, resource.Id);
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