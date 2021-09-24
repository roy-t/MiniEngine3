using System;

namespace Mini.Engine.DirectX
{
    public interface IContent : IDisposable
    {
        void Reload();
        string FileName { get; }
    }
}
