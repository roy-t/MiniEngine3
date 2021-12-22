using System;
using Mini.Engine.DirectX;

namespace Mini.Engine.Content;

public interface IContent : IDisposable
{
    ContentId Id { get; }
    void Reload(Device device);
}
