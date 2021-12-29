using System;
using Vortice.Direct3D11;

namespace Mini.Engine.DirectX.Buffers;

public sealed class InputLayout : IDisposable
{
    internal InputLayout(ID3D11InputLayout iD3D11InputLayout)
    {
        this.ID3D11InputLayout = iD3D11InputLayout;
    }

    internal ID3D11InputLayout ID3D11InputLayout { get; }

    public void Dispose()
    {
        this.ID3D11InputLayout.Dispose();
    }
}
