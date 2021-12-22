using Mini.Engine.DirectX;

namespace Mini.Engine.Content;

internal interface IContentLoader<T>
{
    T Load(Device device, ContentId id);
    void Unload(T content);
}
