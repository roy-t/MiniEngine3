using System;

namespace Mini.Engine.DirectX;

public interface IContent : IDisposable
{
    string FileName { get; }
    void Reload(Device device);
}
