using System;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Resources;

public interface ITexture2D : IDisposable
{
    internal ID3D11ShaderResourceView ShaderResourceView { get; }
    internal ID3D11Texture2D Texture { get; }
    public int Width { get; }
    public int Height { get; }
    public Format Format { get; }
}
