using Mini.Engine.DirectX;

namespace Mini.Engine.Content;

internal interface IContentLoader<T>
{
    T Load(Device device, string fileName);
    void Unload(T content);
}
