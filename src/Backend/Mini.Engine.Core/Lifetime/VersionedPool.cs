using LibGame.Collections;

namespace Mini.Engine.Core.Lifetime;
internal sealed class VersionedPool
{
    private record Record(IDisposable Element, int Version);

    private const int InitialCapacity = 100;
    private readonly ReferencePool<Record> Records;

    public VersionedPool()
    {
        this.Records = new ReferencePool<Record>(InitialCapacity);
    }

    public IDisposable this[ILifetime resource]
    {
        get
        {
            if (this.IsValid(resource))
            {
                return this.Records[resource.Id].Element;
            }
            throw new Exception($"The resource pointed to by {resource} no longer exists");
        }
    }

    public bool IsValid(ILifetime resource)
    {
        return this.Records.IsOccupied(resource.Id) && this.Records[resource.Id].Version == resource.Version;
    }

    public ILifetime<T> Add<T>(T resource, int version)
        where T : IDisposable
    {
        var index = this.Records.Add(new Record(resource, version));
        return new StandardLifetime<T>(index, version);
    }

    public void DisposeAll(int version)
    {
        for(var i = 0; i < this.Records.Capacity; i++)
        {
            if (this.Records.IsOccupied(i))
            {
                var record = this.Records[i];
                if (record.Version == version)
                {
                    record.Element.Dispose();
                    this.Records.Remove(i);
                }
            }
        }        
    }  
}
