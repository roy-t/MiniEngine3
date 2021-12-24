﻿using System;
using Mini.Engine.DirectX;
using Vortice.DXGI;

namespace Mini.Engine.Content.Textures;

public sealed record TextureData(ContentId Id, int Width, int Height, int Pitch, Format Format, byte[] Data)
    : IContentData;

internal sealed class Texture2DContent : Texture2D, IContent
{
    private readonly IContentDataLoader<TextureData> Loader;

    public Texture2DContent(ContentId id, Device device, IContentDataLoader<TextureData> loader, TextureData data)
        : base(device, data.Width, data.Height, data.Format, true, id.ToString())
    {
        this.Loader = loader;
        this.Id = id;

        this.Width = data.Width;
        this.Height = data.Height;
        this.Format = data.Format;

        device.ID3D11DeviceContext.UpdateSubresource(data.Data, this.Texture, 0, data.Pitch, 0);
    }

    public ContentId Id { get; }
    public int Width { get; }
    public int Height { get; }
    public Format Format { get; }

    public void Reload(Device device)
    {
        var data = this.Loader.Load(device, this.Id);
        this.Reload(device, data.Width, data.Height, data.Pitch, data.Format, data.Data);
    }

    private void Reload(Device device, int width, int height, int pitch, Format format, byte[] data)
    {
        if (width != this.Width || height != this.Height || format != this.Format)
        {
            throw new NotSupportedException($"Cannot reload {this.Id}, dimensions or format have changed");
        }
        device.ID3D11DeviceContext.UpdateSubresource(data, this.Texture, 0, pitch, 0);
    }
}