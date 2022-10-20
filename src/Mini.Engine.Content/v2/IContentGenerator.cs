using Mini.Engine.Content.v2.Serialization;
using Mini.Engine.DirectX;

namespace Mini.Engine.Content.v2;

internal interface IContentGenerator<T>
    where T : IContent
{
    void Generate(ContentId id, ContentRecord meta, TrackingVirtualFileSystem fileSystem, Stream stream);
    T Load(Device device, ContentId id, ContentBlob blob);
}

