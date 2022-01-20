using System;
using System.Numerics;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Resources;

public interface ITexture2D : IDisposable
{
    internal ID3D11ShaderResourceView ShaderResourceView { get; }
    internal ID3D11Texture2D Texture { get; }
    public Vector2 Dimensions { get; } // TODO change to width and height, integers?
    public Format Format { get; }
}
