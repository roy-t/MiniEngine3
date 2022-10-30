using Mini.Engine.Content.v2.Serialization;
using Mini.Engine.Core.Lifetime;

namespace Mini.Engine.Content.v2;

public interface IContentTypeCache<T>
    where T : IDisposable, IContent
{
    public bool TryGetValue(ContentId id, out ILifetime<T> value);
    public void Store(ContentId id, ILifetime<T> value);
}

public interface IContentTypeManager
{
    int Version { get; }
    void Reload(IContent original, ContentWriterReader writerReader, TrackingVirtualFileSystem fileSystem);
}

public interface IContentTypeManager<TContent, TSettings> : IContentTypeManager
    where TContent : IDisposable, IContent
{
    void Generate(ContentId id, TSettings settings, ContentWriter writer, TrackingVirtualFileSystem fileSystem);
    TContent Load(ContentId contentId, ContentHeader header, ContentReader reader);
    IContentTypeCache<TContent> Cache { get; }
}
