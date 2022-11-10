using Mini.Engine.Content.v2.Serialization;
using Mini.Engine.Core.Lifetime;

namespace Mini.Engine.Content.v2;

public interface IContentTypeCache<T>
    where T : class
{
    public bool TryGetValue(ContentId id, out T value);
    public void Store(ContentId id, T value);
}

public interface IContentProcessor
{
    int Version { get; }
    void Reload(IContent original, ContentWriterReader writerReader, TrackingVirtualFileSystem fileSystem);
}

public interface IContentProcessor<TContent, TWrapped, TSettings> : IContentProcessor
    where TContent : class
    where TWrapped : IContent, TContent
{
    void Generate(ContentId id, TSettings settings, ContentWriter writer, TrackingVirtualFileSystem fileSystem);
    TContent Load(ContentId contentId, ContentHeader header, ContentReader reader);
    TWrapped Wrap(ContentId id, TContent content, TSettings settings, ISet<string> dependencies);
}


public interface IUnmanagedContentProcessor<TContent, TWrapped, TSettings> : IContentProcessor<TContent, TWrapped, TSettings>
    where TContent : class, IDisposable
    where TWrapped : IContent, TContent
{
    IContentTypeCache<ILifetime<TContent>> Cache { get; }    
}

public interface IManagedContentProcessor<TContent, TWrapped, TSettings> : IContentProcessor<TContent, TWrapped, TSettings>
    where TContent : class
    where TWrapped : IContent, TContent
{
    IContentTypeCache<TContent> Cache { get; }
}
