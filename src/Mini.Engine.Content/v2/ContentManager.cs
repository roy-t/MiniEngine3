using Mini.Engine.Configuration;
using Mini.Engine.Content.Textures;
using Mini.Engine.Content.v2.Textures;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.IO;

namespace Mini.Engine.Content.v2;

[Service]
public sealed class ContentManager : IDisposable
{
    private readonly Device Device;
    private readonly ContentCache<TextureContent> TextureCache;

    private readonly Stack<ContentFrame> ContentStack;

    public ContentManager(Device device, IVirtualFileSystem fileSystem)
    {
        this.Device = device;
        this.TextureCache = new ContentCache<TextureContent>(device, new Textures.TextureLoader(), fileSystem);

        this.ContentStack = new Stack<ContentFrame>();
        this.ContentStack.Push(new ContentFrame("Root"));
    }

    public IResource<ITexture> LoadTexture(string path, string key = "", TextureLoaderSettings? settings = null)
    {
        var id = new ContentId(path, key);
        var content = this.TextureCache.Load(id, new ContentRecord(settings));

        return this.RegisterContentResource(content);        
    }

    private IResource<T> RegisterContentResource<T>(T content)
        where T : IDeviceResource, IContent
    {
        this.ContentStack.Peek().Content.Add(content);
        return this.Device.Resources.Add(content);        
    }

    private void Pop()
    {
        var frame = this.ContentStack.Pop();
        foreach (var content in frame.Content)
        {
            switch (content)
            {
                case TextureContent texture:
                    this.TextureCache.Unload(texture.Id);
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Unexpected content type: {content.GetType().FullName}");
            }
        }
    }


    public void Dispose()
    {
        while (this.ContentStack.Count > 0)
        {
            this.Pop();
        }
    }


}
