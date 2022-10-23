using System.Diagnostics.CodeAnalysis;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Surfaces;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.Content.v2.Textures;
public sealed class TextureContent : ITexture, IContent
{
    private ITexture original;

    public TextureContent(ContentId id, ITexture original, ContentRecord meta, ISet<string> dependencies)
    {
        this.Id = id;
        this.GeneratorKey = string.Empty;
        this.Meta = meta;
        this.Dependencies = dependencies;

        this.Reload(original);

        this.Name = DebugNameGenerator.GetName(id.ToString(), "Texture", string.Empty, this.Format);
    }

    [MemberNotNull(nameof(original))]
    public void Reload(ITexture original)
    {
        this.Dispose();
        this.original = original;
    }

    public ContentId Id { get; }
    public string GeneratorKey { get; set; }
    public ContentRecord Meta { get; }
    public string Name { get; }

    public ImageInfo ImageInfo => this.original.ImageInfo;
    public MipMapInfo MipMapInfo => this.original.MipMapInfo;

    public Format Format => this.original.ImageInfo.Format;
    public int DimX => this.original.ImageInfo.DimX;
    public int DimY => this.original.ImageInfo.DimY;
    public int DimZ => this.original.ImageInfo.DimZ;
    public int MipMapLevels => this.original.MipMapInfo.Levels;

    ID3D11ShaderResourceView ISurface.ShaderResourceView => this.original.ShaderResourceView;
    ID3D11Texture2D ISurface.Texture => this.original.Texture;

    public ISet<string> Dependencies { get; }

    public void Dispose()
    {
        this.original?.Dispose();
    }
}
