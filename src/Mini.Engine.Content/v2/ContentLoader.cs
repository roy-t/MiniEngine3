using Mini.Engine.Content.v2.Serialization;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.IO;
using Serilog;

namespace Mini.Engine.Content.v2;
internal class ContentLoader
{
    private readonly LifetimeManager LifetimeManager;
    private readonly IVirtualFileSystem FileSystem;
    private readonly HotReloader HotReloader;

    public ContentLoader(ILogger logger, LifetimeManager lifetimeManager, IVirtualFileSystem fileSystem)
    {
        this.LifetimeManager = lifetimeManager;
        this.FileSystem = fileSystem;
        this.HotReloader = new HotReloader(logger, fileSystem);
    }

    public TContent Load<TContent, TWrapped, TSettings>(IManagedContentProcessor<TContent, TWrapped, TSettings> processor, ContentId id, TSettings settings)
        where TContent : class
        where TWrapped : IContent, TContent
    {        
        if (processor.Cache.TryGetValue(id, out var material))
        {
            return material;
        }

        var path = PathGenerator.GetPath(id);
        if (this.FileSystem.Exists(path))
        {
            using var rStream = this.FileSystem.OpenRead(path);
            using var reader = new ContentReader(rStream);
            var header = reader.ReadHeader();
            if (ContentProcessorUtilities.IsContentUpToDate(processor.Version, header, this.FileSystem))
            {
                var content = processor.Load(id, header, reader);
#if DEBUG
                var wrapped = processor.Wrap(id, content, settings, header.Dependencies);
                this.HotReloader.Register(wrapped, processor);
                content = wrapped;
#endif
                processor.Cache.Store(id, content);
                return content;
            }
        }

        // 3. Generate, store, load from disk                
        using (var rwStream = this.FileSystem.CreateWriteRead(path))
        {
            using var writer = new ContentWriter(rwStream);
            processor.Generate(id, settings, writer, new TrackingVirtualFileSystem(this.FileSystem));
        }

        return this.Load(processor, id, settings);
    }


    public ILifetime<TContent> Load<TContent, TWrapped, TSettings>(IUnmanagedContentProcessor<TContent, TWrapped, TSettings> processor, ContentId id, TSettings settings)
        where TContent : class, IDisposable
        where TWrapped : IContent, TContent
    {
        // 1. Return existing reference        
        if (processor.Cache.TryGetValue(id, out var t))
        {
            return t;
        }

        // 2. Load from disk
        var path = PathGenerator.GetPath(id);
        if (this.FileSystem.Exists(path))
        {
            using var rStream = this.FileSystem.OpenRead(path);
            using var reader = new ContentReader(rStream);
            var header = reader.ReadHeader();
            if (ContentProcessorUtilities.IsContentUpToDate(processor.Version, header, this.FileSystem))
            {
                var content = processor.Load(id, header, reader);
#if DEBUG
                var wrapped = processor.Wrap(id, content, settings, header.Dependencies);
                this.HotReloader.Register(wrapped, processor);
                content = wrapped;
#endif
                var resource = this.RegisterContentResource(content);
                processor.Cache.Store(id, resource);
                return resource;
            }
        }

        // 3. Generate, store, load from disk                
        using (var rwStream = this.FileSystem.CreateWriteRead(path))
        {
            using var writer = new ContentWriter(rwStream);
            processor.Generate(id, settings, writer, new TrackingVirtualFileSystem(this.FileSystem));
        }

        return this.Load(processor, id, settings);
    }

    private ILifetime<T> RegisterContentResource<T>(T content)
        where T : IDisposable
    {
        return this.LifetimeManager.Add(content);
    }

    public void ReloadChangedContent()
    {
        this.HotReloader.ReloadChangedContent();
    }
}
