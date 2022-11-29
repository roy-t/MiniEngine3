using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime;
using Mini.Engine.Content.Serialization;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.IO;
using Serilog;

namespace Mini.Engine.Content;
internal class ContentLoader
{
    private readonly ILogger Logger;
    private readonly LifetimeManager LifetimeManager;
    private readonly IVirtualFileSystem FileSystem;
    private readonly HotReloader HotReloader;
    

    public ContentLoader(ILogger logger, LifetimeManager lifetimeManager, IVirtualFileSystem fileSystem)
    {
        this.Logger = logger.ForContext<ContentLoader>();

        this.LifetimeManager = lifetimeManager;
        this.FileSystem = fileSystem;
        this.HotReloader = new HotReloader(lifetimeManager, logger, fileSystem);
    }   

    public ILifetime<TContent> Load<TContent, TWrapped, TSettings>(IContentProcessor<TContent, TWrapped, TSettings> processor, ContentId id, TSettings settings)
        where TContent : IDisposable
        where TWrapped : IContent, TContent
    {
        if (processor.Cache.TryGetValue(id, out var t))
        {
            return t;
        }

        if (this.TryLoadSerializedContent(processor, id, out var header, out var content))
        {
            content = this.WrapInDebug(processor, id, settings, header, content);
            var resource = this.RegisterContentResource(content, processor);
            processor.Cache.Store(id, resource);
            return resource;
        }

        this.GenerateSerializedContent(processor, id, settings);
        return this.Load(processor, id, settings);
    }

    private bool TryLoadSerializedContent<TContent, TWrapped, TSettings>(IContentProcessor<TContent, TWrapped, TSettings> processor, ContentId id, [NotNullWhen(true)] out ContentHeader? header, [NotNullWhen(true)] out TContent? content)
        where TContent : IDisposable
        where TWrapped : IContent, TContent
    {
        var path = PathGenerator.GetPath(id);
        if (this.FileSystem.Exists(path))
        {
            try
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
            catch(Exception e)
            {
                this.Logger.Warning(e, "Clould not load file {@file}, rebuilding", path);
            }
        }

        header = default;
        content = default;
        return false;
    }

    private TContent WrapInDebug<TContent, TWrapped, TSettings>(IContentProcessor<TContent, TWrapped, TSettings> processor, ContentId id, TSettings settings, ContentHeader header, TContent content)
        where TContent : IDisposable
        where TWrapped : IContent, TContent
    {
#if DEBUG
        var wrapped = processor.Wrap(id, content, settings, header.Dependencies);        
        content = wrapped;
#endif
        return content;
    }

    private void GenerateSerializedContent<TContent, TWrapped, TSettings>(IContentProcessor<TContent, TWrapped, TSettings> processor, ContentId id, TSettings settings)
        where TContent : IDisposable
        where TWrapped : IContent, TContent
    {
        var path = PathGenerator.GetPath(id);
        using var rwStream = this.FileSystem.CreateWriteRead(path);
        using var writer = new ContentWriter(rwStream);
        processor.Generate(id, settings, writer, new TrackingVirtualFileSystem(this.FileSystem));
    }

    private ILifetime<TContent> RegisterContentResource<TContent, TWrapped, TSettings>(TContent content, IContentProcessor<TContent, TWrapped, TSettings> processor)
        where TContent : IDisposable
        where TWrapped : IContent, TContent
    {

        var lifetime = this.LifetimeManager.Add(content);
#if DEBUG
        this.HotReloader.Register((ILifetime<IDisposable>)lifetime, processor);
#endif
        return lifetime;
    }

    public void ReloadChangedContent()
    {
        this.HotReloader.ReloadChangedContent();
    }
    
    public void AddReloadCallback(ContentId id, Action callback)
    {
        this.HotReloader.AddReloadCallback(id, callback);
    }
}
