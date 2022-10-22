using Mini.Engine.Configuration;
using Mini.Engine.Content.Textures;
using Mini.Engine.Content.v2.Textures;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.IO;
using Serilog;

namespace Mini.Engine.Content.v2;

[Service]
public sealed class ContentManager : IDisposable
{
    private readonly Device Device;
    private readonly ContentCache<TextureContent> TextureCache;
    private readonly ContentStack ContentStack;
    private readonly HotReloader HotReloader; // WIP test reloading!

    public ContentManager(ILogger logger, Device device, Textures.TextureGenerator textureLoader, IVirtualFileSystem fileSystem)
    {
        this.Device = device;
        this.TextureCache = new ContentCache<TextureContent>(textureLoader, fileSystem);
        this.ContentStack = new ContentStack(this.TextureCache);

        this.HotReloader = new HotReloader(logger, this.ContentStack, fileSystem, textureLoader);
    }

    public IResource<ITexture> LoadTexture(string path, string key = "", TextureLoaderSettings? settings = null)
    {
        var id = new ContentId(path, key);
        var content = this.TextureCache.Load(id, new ContentRecord(settings));
        this.HotReloader.Register(content);
        return this.RegisterContentResource(content);        
    }

    private IResource<T> RegisterContentResource<T>(T content)
        where T : IDeviceResource, IContent
    {
        this.ContentStack.Add(content);
        return this.Device.Resources.Add(content);        
    }

    public void Pop()
    {
        this.ContentStack.Pop();
    }

    public void Push(string frameName)
    {
        this.ContentStack.Push(frameName);
    }

    public void Dispose()
    {
        this.ContentStack.Clear();
    }

    public void ReloadChangedContent()
    {
        this.HotReloader.ReloadChangedContent();
    }
}
