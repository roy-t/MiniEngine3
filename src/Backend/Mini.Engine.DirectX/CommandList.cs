using System;
using Vortice.Direct3D11;

namespace Mini.Engine.DirectX
{
    public sealed class CommandList : IDisposable
    {
        internal CommandList(ID3D11CommandList iD3D11CommandList)
        {
            this.ID3D11CommandList = iD3D11CommandList;
        }

        internal ID3D11CommandList ID3D11CommandList { get; }

        public void Dispose()
        {
            this.ID3D11CommandList.Dispose();
        }
    }
}
