using System.Collections;

namespace Mini.Engine.DirectX;

public interface IDeviceResource : IDisposable
{

}

public interface IResource
{
    int Id { get; }
}

public interface IResource<out T> : IResource
    where T : IDeviceResource
{

}

internal readonly record struct Resource<T>(int Id, bool Set) : IResource<T>
    where T : IDeviceResource;

public sealed class ResourceManager
{
    private readonly ResourcePool Resources;

    public ResourceManager()
    {
        this.Resources = new ResourcePool();
    }

    public T Get<T>(IResource<T> id)
        where T : IDeviceResource
    {
        if (id == default)
        {
            throw new Exception("Unitialized resource passed");
        }

        return (T)this.Resources[id.Id];
    }

    public IResource<T> Add<T>(T resource)
        where T : IDeviceResource
    {        
        var id = this.Resources.Add(resource);

        return new Resource<T>(id, true);
    }

    public void Dispose(IResource id)
    {
        this.Resources.Remove(id.Id, out var resource);
        resource?.Dispose();
    }
}


internal sealed class ResourcePool
{
    private readonly BitArray Occupancy;
    private IDeviceResource?[] pool;

    private int lowestUnusedSlot = 0;
    private int highestUsedSlot = -1;

    private int count;

    public ResourcePool()
    {
        this.Occupancy = new BitArray(100);
        this.pool = new IDeviceResource?[100];
    }

    public IDeviceResource this[int index]
    {
        get
        {
            if (this.Occupancy[index] == false)
            {
                throw new IndexOutOfRangeException();
            }
            return this.pool[index]!;
        }
    }

    public int Add(IDeviceResource resource)
    {
        this.EnsureCapacity(this.count + 1);

        var index = this.lowestUnusedSlot;
        this.lowestUnusedSlot = this.IndexOfFirstUnused(this.lowestUnusedSlot + 1);
        this.highestUsedSlot = Math.Max(this.highestUsedSlot, index);

        this.pool[index] = resource;
        this.Occupancy[index] = true;
        this.count += 1;
        return index;
    }

    public void Remove(int index, out IDeviceResource resource)
    {
        resource = this[index];
        this.pool[index] = null;
        this.Occupancy[index] = false;
        if (index == this.highestUsedSlot)
        {
            this.highestUsedSlot = this.IndexOfLastUsed(index - 1);
        }

        this.lowestUnusedSlot = Math.Min(this.lowestUnusedSlot, index);
    }

    private void EnsureCapacity(int capacity)
    {
        if (capacity >= this.pool.Length)
        {
            var newCapacity = Math.Max(capacity, this.pool.Length * 2);

            this.Occupancy.Length = newCapacity;
            Array.Resize(ref this.pool, newCapacity);
        }
    }

    private int IndexOfFirstUnused(int minIndex)
    {
        if (minIndex < this.pool.Length)
        {
            for (var i = minIndex; i < this.pool.Length; i++)
            {
                if (this.Occupancy[i] == false)
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
                if (this.Occupancy[i] == true)
                {
                    return i;
                }
            }
        }

        return -1;
    }
}