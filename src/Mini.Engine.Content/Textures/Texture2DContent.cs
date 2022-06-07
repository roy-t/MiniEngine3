using System.Diagnostics.CodeAnalysis;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.Content.Textures;

internal sealed record TextureData(ContentId Id, ImageInfo ImageInfo, MipMapInfo MipMapInfo, IReadOnlyList<byte[]> MipMaps)
    : IContentData;

internal sealed class Texture2DContent : ITexture2D, IContent
{
    private readonly IContentDataLoader<TextureData> Loader;
    private readonly ILoaderSettings Settings;
    private ITexture2D texture;

    public Texture2DContent(ContentId id, Device device, IContentDataLoader<TextureData> loader, ILoaderSettings settings)
    {
        this.Id = id;
        this.Loader = loader;
        this.Settings = settings;
        this.Reload(device);
    }

    public ContentId Id { get; }
    public string Name => this.texture.Name;
    public int Width => this.texture.Width;
    public int Height => this.texture.Height;
    public Format Format => this.texture.Format;
    public int Levels => this.texture.Levels;
    public int Length => this.texture.Length;

    ID3D11ShaderResourceView ITexture.ShaderResourceView => this.texture.ShaderResourceView;
    ID3D11Texture2D ITexture.Texture => this.texture.Texture;

    [MemberNotNull(nameof(texture))]
    public void Reload(Device device)
    {
        this.texture?.Dispose();

        var data = this.Loader.Load(device, this.Id, this.Settings);
        var texture = new Texture2D(device, data.ImageInfo, data.MipMapInfo, data.Id.ToString(), string.Empty);
        if (data.MipMaps.Count == 1)
        {
            texture.SetPixels<byte>(device, data.MipMaps[0]);
        }
        else
        {
            for(var i = 0; i < data.MipMaps.Count; i++)
            {                
                texture.SetMipMapPixels<byte>(device, data.MipMaps[i], i);
            }
        }

        this.texture = texture;        
    }

    public void Dispose()
    {
        this.texture.Dispose();
    }

    public override string ToString()
    {
        return $"Texture2D: {this.Id}";
    }
}
