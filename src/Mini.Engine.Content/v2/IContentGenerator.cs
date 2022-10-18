using Mini.Engine.DirectX;

namespace Mini.Engine.Content.v2;

public interface IContentGenerator
{
    byte[] Generate(ContentId id);
    IResource Upload(Device device, byte[] data);
}

public abstract class ContentGenerator<T> : IContentGenerator
    where T : IDeviceResource
{
    public abstract byte[] Generate(ContentId id);

    public abstract IResource<T> Upload(Device device, byte[] data);

    IResource IContentGenerator.Upload(Device device, byte[] data)
    {
        return this.Upload(device, data);
    }
}

