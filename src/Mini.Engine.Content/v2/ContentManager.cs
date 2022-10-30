using Mini.Engine.Configuration;
using Mini.Engine.Content.Textures;
using Mini.Engine.Content.v2.Serialization;
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
    private readonly LifetimeManager LifetimeManager;
    private readonly IVirtualFileSystem FileSystem;
    private readonly HotReloader HotReloader;

    private readonly TextureGenerator TextureGenerator;

    public ContentManager(ILogger logger, Device device, LifetimeManager lifetimeManager, IVirtualFileSystem fileSystem)
    {
        this.LifetimeManager = lifetimeManager;
        this.FileSystem = fileSystem;
        this.HotReloader = new HotReloader(logger, fileSystem);

        this.TextureGenerator = new TextureGenerator(device);
    }

    public ILifetime<ITexture> LoadTexture(string path, TextureLoaderSettings settings)
    {
        return this.Load(this.TextureGenerator, settings, path);
    }

    public ILifetime<TContent> Load<TContent, TSettings>(IContentTypeManager<TContent, TSettings> manager, TSettings settings, string path, string? key = null)
        where TContent : IDisposable, IContent
    {
        return this.Load(manager, settings, new ContentId(path, key ?? string.Empty));
    }

    public ILifetime<TContent> Load<TContent, TSettings>(IContentTypeManager<TContent, TSettings> manager, TSettings settings, ContentId id)
        where TContent : IDisposable, IContent
    {
        // 1. Return existing reference        
        if (manager.Cache.TryGetValue(id, out var t))
        {
            return t;
        }

        // 2. Load from disk
        var path = id.Path + Constants.Extension;
        if (this.FileSystem.Exists(path))
        {
            using (var rStream = this.FileSystem.OpenRead(path))
            {
                using var reader = new ContentReader(rStream);
                var header = reader.ReadHeader();
                if (this.IsCurrent(manager, header))
                {                    
                    var content = manager.Load(id, header, reader);
                    this.HotReloader.Register(content, manager);

                    var resource = this.RegisterContentResource(content);
                    manager.Cache.Store(id, resource);

                    return resource;
                }
            }
        }

        // 3. Generate, store, load from disk                
        using (var rwStream = this.FileSystem.CreateWriteRead(path))
        {
            using var writer = new ContentWriter(rwStream);            
            manager.Generate(id, settings, writer, new TrackingVirtualFileSystem(this.FileSystem));
        }

        return this.Load(manager, settings, id);
    }

    private bool IsCurrent(IContentTypeManager manager, ContentHeader header)
    {
        if (header.Version != manager.Version)
        {
            return false;
        }

        var lastWrite = header.Dependencies
            .Select(d => this.FileSystem.GetLastWriteTime(d))
            .Append(header.Timestamp).Max();

        return lastWrite <= header.Timestamp;
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
