using Mini.Engine.Core;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Resources;

public enum CubeMapFace
{
    PositiveX = 0,
    NegativeX = 1,
    PositiveY = 2,
    NegativeY = 3,
    PositiveZ = 4,
    NegativeZ = 5
}

public sealed class TextureCube : ITexture2D
{
    public TextureCube(Device device, int resolution, Format format, bool generateMipMaps, string user, string meaning)
    {
        this.Resolution = resolution;
        this.Format = format;

        this.MipMapSlices = generateMipMaps ? Dimensions.MipSlices(resolution) : 1;
        this.Texture = Textures.Create(device, resolution, resolution, format, BindFlags.ShaderResource | BindFlags.RenderTarget, ResourceOptionFlags.TextureCube, 6, this.MipMapSlices, generateMipMaps, user, meaning);
        this.ShaderResourceView = ShaderResourceViews.Create(device, this.Texture, format, ShaderResourceViewDimension.TextureCube, user, meaning);
        
        this.Name = DebugNameGenerator.GetName(user, "TextureCube", meaning, format);
    }

    public string Name { get; }
    public const int Faces = 6;
    public int Resolution { get; }
    public int Width => Resolution;
    public int Height => Resolution;
    public Format Format { get; }
    public int MipMapSlices { get; }
    public int ArraySize => 1;

    internal ID3D11ShaderResourceView ShaderResourceView { get; }
    internal ID3D11Texture2D Texture { get; }

    ID3D11ShaderResourceView ITexture.ShaderResourceView => this.ShaderResourceView;
    ID3D11Texture2D ITexture.Texture => this.Texture;

    public void Dispose()
    {
        this.ShaderResourceView.Dispose();
        this.Texture.Dispose();
    }
}
