using System;
using System.Diagnostics.CodeAnalysis;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.Content.Textures;

internal sealed record TextureData(ContentId Id, int Width, int Height, int Pitch, Format Format, byte[] Data)
    : IContentData;

internal sealed class Texture2DContent : ITexture2D, IContent
{
    private readonly IContentDataLoader<TextureData> Loader;
    private Texture2D Texture;

    public Texture2DContent(ContentId id, Device device, IContentDataLoader<TextureData> loader)
    {
        this.Id = id;
        this.Loader = loader;

        this.Reload(device);
    }

    public ContentId Id { get; }

    ID3D11ShaderResourceView ITexture2D.ShaderResourceView => this.Texture.ShaderResourceView;
    ID3D11Texture2D ITexture2D.Texture => this.Texture.Texture;

    [MemberNotNull(nameof(Texture))]
    public void Reload(Device device)
    {
        this.Texture?.Dispose();

        var data = this.Loader.Load(device, this.Id);
        this.Texture = new Texture2D(device, data.Data, data.Width, data.Height, data.Format, true, data.Id.ToString());
    }

    public void Dispose()
    {
        this.Texture.Dispose();
    }
}
