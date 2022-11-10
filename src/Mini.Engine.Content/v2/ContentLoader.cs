using System.Diagnostics.CodeAnalysis;
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

        if (this.TryLoadSerializedContent(processor, id, out var header, out var content))
        {
            content = this.WrapInDebug(processor, id, settings, header, content);
            processor.Cache.Store(id, content);
            return content;
        }

        this.GenerateSerializedContent(processor, id, settings);
        return this.Load(processor, id, settings);
    }

    public ILifetime<TContent> Load<TContent, TWrapped, TSettings>(IUnmanagedContentProcessor<TContent, TWrapped, TSettings> processor, ContentId id, TSettings settings)
        where TContent : class, IDisposable
        where TWrapped : IContent, TContent
    {
        if (processor.Cache.TryGetValue(id, out var t))
        {
            return t;
        }

        if (this.TryLoadSerializedContent(processor, id, out var header, out var content))
        {
            content = this.WrapInDebug(processor, id, settings, header, content);
            var resource = this.RegisterContentResource(content);
            processor.Cache.Store(id, resource);
            return resource;
        }

        this.GenerateSerializedContent(processor, id, settings);
        return this.Load(processor, id, settings);
    }

    private bool TryLoadSerializedContent<TContent, TWrapped, TSettings>(IContentProcessor<TContent, TWrapped, TSettings> processor, ContentId id, [NotNullWhen(true)] out ContentHeader? header, [NotNullWhen(true)] out TContent? content)
        where TContent : class
        where TWrapped : IContent, TContent
    {
        var path = PathGenerator.GetPath(id);
        if (this.FileSystem.Exists(path))
        {
            using var rStream = this.FileSystem.OpenRead(path);
            using var reader = new ContentReader(rStream);
            header = reader.ReadHeader();
            if (ContentProcessorValidation.IsContentUpToDate(processor.Version, header, this.FileSystem))
            {
                content = processor.Load(id, header, reader);
                return true;
            }
        }

        header = null;
        content = null;
        return false;
    }

    private TContent WrapInDebug<TContent, TWrapped, TSettings>(IContentProcessor<TContent, TWrapped, TSettings> processor, ContentId id, TSettings settings, ContentHeader header, TContent content)
        where TContent : class
        where TWrapped : IContent, TContent
    {
#if DEBUG
        var wrapped = processor.Wrap(id, content, settings, header.Dependencies);
        this.HotReloader.Register(wrapped, processor);
        content = wrapped;
#endif
        return content;
    }

    private void GenerateSerializedContent<TContent, TWrapped, TSettings>(IContentProcessor<TContent, TWrapped, TSettings> processor, ContentId id, TSettings settings)
        where TContent : class
        where TWrapped : IContent, TContent
    {
        var path = PathGenerator.GetPath(id);
        using var rwStream = this.FileSystem.CreateWriteRead(path);
        using var writer = new ContentWriter(rwStream);
        processor.Generate(id, settings, writer, new TrackingVirtualFileSystem(this.FileSystem));
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
