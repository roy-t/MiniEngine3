using System.Diagnostics.CodeAnalysis;
using Mini.Engine.Content.Textures;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Surfaces;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.Content.v2.Textures;
public sealed class TextureContent : ITexture, IContent
{
    private ID3D11ShaderResourceView shaderResourceView;
    private ID3D11Texture2D texture;

    public TextureContent(ContentId id, TextureData data, ContentRecord meta, string generatorKey, ISet<string> dependencies)
    {
        this.Id = id;
        this.GeneratorKey = generatorKey;
        this.Meta = meta;
        this.Dependencies = dependencies;

        this.Reload(data);
        
        this.Name = DebugNameGenerator.GetName(id.ToString(), "Texture", string.Empty, this.Format);        
    }

    [MemberNotNull(nameof(shaderResourceView), nameof(texture))]
    public void Reload(TextureData data)
    {
        this.Dispose();

        this.ImageInfo = data.ImageInfo;
        this.MipMapInfo = data.MipMapInfo;
        this.texture = data.Texture;
        this.shaderResourceView = data.View;
    }

    public ContentId Id { get; }
    public string GeneratorKey { get; }
    public ContentRecord Meta { get; }
    public string Name { get; }

    public ImageInfo ImageInfo { get; set; }
    public MipMapInfo MipMapInfo { get; set; }

    public Format Format => this.ImageInfo.Format;
    public int DimX => this.ImageInfo.DimX;
    public int DimY => this.ImageInfo.DimY;
    public int DimZ => this.ImageInfo.DimZ;
    public int MipMapLevels => this.MipMapInfo.Levels;

    ID3D11ShaderResourceView ISurface.ShaderResourceView => this.shaderResourceView!;
    ID3D11Texture2D ISurface.Texture => this.texture!;

    public ISet<string> Dependencies { get; }

    public void Dispose()
    {
        this.shaderResourceView?.Dispose();
        this.texture?.Dispose();
    }
}
