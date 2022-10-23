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
    private readonly IVirtualFileSystem FileSystem;
    private readonly ContentStack ContentStack;
    private readonly HotReloader HotReloader;

    private readonly Dictionary<string, IContentCache> Caches;

    public ContentManager(ILogger logger, Device device, IVirtualFileSystem fileSystem, IReadOnlyList<IContentGenerator> generators)
    {
        this.Caches = new Dictionary<string, IContentCache>();
        foreach (var generator in generators)
        {
            var cache = generator.CreateCache(fileSystem);
            this.Caches.Add(generator.GeneratorKey, cache);
        }

        this.Device = device;
        this.FileSystem = fileSystem;
        this.ContentStack = new ContentStack(this.Caches);
        this.HotReloader = new HotReloader(logger, this.ContentStack, fileSystem, generators);
    }

    public IResource<T> Load<T>(string generatorKey, string path, string key = "", ContentRecord? record = null)
        where T : IDeviceResource, IContent
    {
        var id = new ContentId(path, key);
        var cache = this.Caches[generatorKey];
        var content = (T)cache.Load(id, record ?? ContentRecord.Default);
        this.HotReloader.Register(content);
        return this.RegisterContentResource(content);
    }

    public IResource<ITexture> LoadTexture(string path, string key = "", TextureLoaderSettings? settings = null)
    {
        return this.Load<TextureContent>(nameof(TextureGenerator), path, key, new ContentRecord(settings));
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
