using Mini.Engine.Content.v2.Serialization;
using Mini.Engine.Core.Lifetime;

namespace Mini.Engine.Content.v2;

public interface IContentCache<T>
    where T : IDisposable, IContent
{
    public bool TryGetValue(ContentId id, out ILifetime<T> value);
    public void Store(ContentId id, ILifetime<T> value);
}

public interface IContentLoader<T>
    where T : IContent
{
    T Load(ContentId contentId, ContentReader reader);
}

public interface IContentWriter<T>
{
    void Generate(ContentId id, T settings, ContentWriter writer, TrackingVirtualFileSystem fileSystem);
}

public interface IContentReloader<T>
    where T : IContent
{
    // Does having a reader and writer  make sense?
    void Reload(T original, ContentWriter writer, ContentReader reader, TrackingVirtualFileSystem fileSystem);
}

public interface IContentUnloader<T>
    where T : IContent
{
    void Unload(T content);
}

public interface IContentTypeManager<TContent, TSettings>
    where TContent : IDisposable, IContent
{
    string Key { get; }
    IContentCache<TContent> GetCache();
    IContentWriter<TSettings> GetWriter();
    IContentLoader<TContent> GetLoader();    
    IContentReloader<TContent> GetReloader();
    IContentUnloader<TContent> GetUnloader();
}
