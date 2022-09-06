using System.Diagnostics.CodeAnalysis;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Surfaces;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.Content.Textures;

internal sealed record TextureData(ContentId Id, ImageInfo ImageInfo, MipMapInfo MipMapInfo, ID3D11Texture2D Texture, ID3D11ShaderResourceView View)
    : IContentData;

internal sealed class Texture2DContent : ITexture, IContent
{
    private readonly IContentDataLoader<TextureData> Loader;
    private readonly ILoaderSettings Settings;

    private ID3D11ShaderResourceView shaderResourceView;
    private ID3D11Texture2D texture;    

    public Texture2DContent(ContentId id, Device device, IContentDataLoader<TextureData> loader, ILoaderSettings settings)
    {
        this.Id = id;
        this.Loader = loader;
        this.Settings = settings;
        this.Name = DebugNameGenerator.GetName(id.ToString(), "Texture2D", string.Empty, this.Format);

        this.Reload(device);
    }

    public ContentId Id { get; }
    public string Name { get; }

    public int DimX { get; private set; }
    public int DimY { get; private set; }        
    public int DimZ { get; private set; }
    public int MipMapLevels { get; private set; }    

    public Format Format { get; private set; }

    ID3D11ShaderResourceView ISurface.ShaderResourceView
    {
        get =>  this.shaderResourceView;
        set { }
    }
    ID3D11Texture2D ISurface.Texture
    {
        get => this.texture;
        set { }
    }

    [MemberNotNull(nameof(shaderResourceView), nameof(texture))]
    public void Reload(Device device)
    {
        this.texture?.Dispose();

        var data = this.Loader.Load(device, this.Id, this.Settings);

        this.DimX = data.ImageInfo.DimX;
        this.DimY = data.ImageInfo.DimY;        
        this.MipMapLevels = data.MipMapInfo.Levels;
        this.DimZ = data.ImageInfo.DimZ;

        this.Format = data.ImageInfo.Format;

        this.texture?.Dispose();
        this.texture = data.Texture;

        this.shaderResourceView?.Dispose();
        this.shaderResourceView = data.View;
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
