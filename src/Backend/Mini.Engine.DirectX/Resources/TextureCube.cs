﻿using System.Numerics;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Resources;

// TODO: see D:\Projects\C#\MiniRTS\src\MiniEngine.Graphics\Utilities\CubeMapUtilities.cs
// make a simple as possible cube texture and then try to render an equirectangular in there
public sealed class TextureCube : ITexture2D
{
    public TextureCube(Device device, int resolution, Format format, bool generateMipMaps, string name)
    {
        this.Width = resolution;
        this.Height = resolution;
        this.Format = format;

        this.Texture = Textures.Create(device, resolution, resolution, format, BindFlags.ShaderResource | BindFlags.RenderTarget, ResourceOptionFlags.TextureCube, 6, generateMipMaps, name);
        this.ShaderResourceView = ShaderResourceViews.Create(device, this.Texture, format, ShaderResourceViewDimension.TextureCube, name);
    }

    public const int Faces = 6;
    public int Width { get; }
    public int Height { get; }
    public Format Format { get; }

    internal ID3D11ShaderResourceView ShaderResourceView { get; }
    internal ID3D11Texture2D Texture { get; }

    ID3D11ShaderResourceView ITexture2D.ShaderResourceView => this.ShaderResourceView;
    ID3D11Texture2D ITexture2D.Texture => this.Texture;

    public void Dispose()
    {
        this.ShaderResourceView.Dispose();
        this.Texture.Dispose();
    }
}
