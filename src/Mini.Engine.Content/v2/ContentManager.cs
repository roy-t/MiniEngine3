using Mini.Engine.Configuration;
using Mini.Engine.Content.Textures;
using Mini.Engine.Content.v2.Textures;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.IO;
using Serilog;

namespace Mini.Engine.Content.v2;

[Service]
public sealed class ContentManager
{
    private readonly Device Device;
    private readonly LifetimeManager LifetimeManager;
    private readonly IVirtualFileSystem FileSystem;
    private readonly HotReloader HotReloader;

    private readonly Dictionary<string, IContentCache> Caches;

    public ContentManager(ILogger logger, LifetimeManager lifetimeManager, Device device, IVirtualFileSystem fileSystem, IReadOnlyList<IContentGenerator> generators)
    {
        this.LifetimeManager = lifetimeManager;
        this.Caches = new Dictionary<string, IContentCache>();
        foreach (var generator in generators)
        {
            var cache = generator.CreateCache(fileSystem);
            this.Caches.Add(generator.GeneratorKey, cache);
        }

        this.Device = device;
        this.FileSystem = fileSystem;
        this.HotReloader = new HotReloader(logger, fileSystem, generators);
    }

    //public IResource<TContent> Load<TContent, TSettings>(IContentTypeManager<TContent, TSettings> manager, ContentId id, TSettings settings)
    //    where TContent : IDeviceResource, IContent
    //{
    //    // 1. Return existing reference
    //    var cache = manager.GetCache();
    //    if (cache.TryGetValue(id, out var t))
    //    {
    //        // We don't need to add things to the content stack if its already there, this also makes unloading easier
    //        return t;
    //    }

    //    // 2. Load from disk
    //    var path = id.Path + Constants.Extension;
    //    if (this.FileSystem.Exists(path))
    //    {
    //        using var rStream = this.FileSystem.OpenRead(path);
    //        using var reader = new ContentReader(rStream);
    //        var common = reader.ReadHeader();
    //        if (this.IsCurrent(common))
    //        {
    //            rStream.Seek(0, SeekOrigin.Begin);
    //            var loader = manager.GetLoader();
    //            var content = loader.Load(id, reader);
    //            var resource = this.RegisterContentResource(content);
    //            cache.Store(id, resource);

    //            return resource;
    //        }
    //    }

    //    // 3. Generate, store, load from disk        
    //    var generator = manager.GetWriter();
    //    using var rwStream = this.FileSystem.CreateWriteRead(path);
    //    using var writer = new ContentWriter(rwStream);
    //    generator.Generate(id, settings, writer, new TrackingVirtualFileSystem(this.FileSystem));

    //    return this.Load(manager, id, settings);
    //}

    //private void Reload(ContentId content)
    //{

    //}

    public ILifetime<T> Load<T>(string generatorKey, string path, string key = "", ContentRecord? record = null)
        where T : IDisposable, IContent
    {
        var id = new ContentId(path, key);
        var cache = this.Caches[generatorKey];
        var content = (T)cache.Load(id, record ?? ContentRecord.Default);
        this.HotReloader.Register(content);
        return this.RegisterContentResource(content);
    }

    public ILifetime<ITexture> LoadTexture(string path, string key = "", TextureLoaderSettings? settings = null)
    {
        return this.Load<TextureContent>(nameof(TextureGenerator), path, key, new ContentRecord(settings));
    }

    private ILifetime<T> RegisterContentResource<T>(T content)
        where T : IDisposable, IContent
    {
        return this.LifetimeManager.Add(content);
    }

    public void ReloadChangedContent()
    {
        this.HotReloader.ReloadChangedContent();
    }
}
