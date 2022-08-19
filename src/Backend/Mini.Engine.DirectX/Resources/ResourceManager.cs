namespace Mini.Engine.DirectX;

public interface IDeviceResource : IDisposable
{

}

public interface IResource
{
    int Id { get; }
    int Version { get; }
}

public interface IResource<out T> : IResource
    where T : IDeviceResource
{

}

internal readonly record struct Resource<T>(int Id, int Version) : IResource<T>
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

        return (T)this.Resources[id];
    }

    public IResource<T> Add<T>(T resource)
        where T : IDeviceResource
    {
        return this.Resources.Add(resource);
    }

    public void Dispose(IResource id)
    {
        this.Resources.Remove(id, out var resource);
        resource?.Dispose();
    }
}
