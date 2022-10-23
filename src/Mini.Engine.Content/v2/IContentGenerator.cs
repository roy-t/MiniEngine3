using Mini.Engine.Content.v2.Serialization;
using Mini.Engine.IO;

namespace Mini.Engine.Content.v2;

public interface IContentGenerator
{
    void Generate(ContentId id, ContentRecord meta, TrackingVirtualFileSystem fileSystem, ContentWriter contentWriter);
    void Reload(IContent original, TrackingVirtualFileSystem fileSystem, Stream rwStream);
    IContentCache CreateCache(IVirtualFileSystem fileSystem);
    string GeneratorKey { get; }
}

public interface IContentGenerator<T> : IContentGenerator
    where T : IContent
{    
    T Load(ContentId id, ContentReader contentReader);
}

