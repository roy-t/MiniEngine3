using Mini.Engine.Content.Caching;
using Mini.Engine.Content.Serialization;
using Mini.Engine.Core.Lifetime;

namespace Mini.Engine.Content;

public interface IContentProcessor
{
    int Version { get; }
    void Reload(IContent original, ContentWriterReader writerReader, TrackingVirtualFileSystem fileSystem);
}

public interface IContentProcessor<TContent, TWrapped, TSettings> : IContentProcessor
    where TContent : IDisposable
    where TWrapped : IContent, TContent
{
    IContentCache<ILifetime<TContent>> Cache { get; }

    void Generate(ContentId id, TSettings settings, ContentWriter writer, TrackingVirtualFileSystem fileSystem);
    TContent Load(ContentId contentId, ContentHeader header, ContentReader reader);
    TWrapped Wrap(ContentId id, TContent content, TSettings settings, ISet<string> dependencies);
}
