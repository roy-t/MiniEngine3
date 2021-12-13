using System;

namespace Mini.Engine.DirectX;

public interface IContent : IDisposable
{
    string Id { get; }
    void Reload(Device device);
}
