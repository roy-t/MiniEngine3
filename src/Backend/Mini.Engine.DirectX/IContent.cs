using System;
using Mini.Engine.IO;

namespace Mini.Engine.DirectX
{
    public interface IContent : IDisposable
    {
        string FileName { get; }
        void Reload(Device device, IVirtualFileSystem fileSystem);
    }
}
