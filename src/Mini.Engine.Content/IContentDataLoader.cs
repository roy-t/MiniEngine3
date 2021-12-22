using Mini.Engine.DirectX;

namespace Mini.Engine.Content;

internal interface IContentDataLoader<T>
    where T : IContentData
{
    T Load(Device device, ContentId id);
}
