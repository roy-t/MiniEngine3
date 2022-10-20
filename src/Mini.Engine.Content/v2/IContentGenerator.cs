using Mini.Engine.Content.v2.Serialization;

namespace Mini.Engine.Content.v2;

public interface IContentGenerator
{
    void Generate(ContentId id, ContentRecord meta, TrackingVirtualFileSystem fileSystem, Stream stream);
    void Reload(IContent original, TrackingVirtualFileSystem fileSystem, Stream rwStream);
    string GeneratorKey { get; }
}

public interface IContentGenerator<T> : IContentGenerator
    where T : IContent
{    
    T Load(ContentId id, ContentBlob blob);
}

