using Vortice.Direct3D11;

namespace Mini.Engine.DirectX.Resources.Surfaces;

public sealed class TextureCube : Texture, ITextureCube
{
    public TextureCube(Device device, string name, ImageInfo image, MipMapInfo mipMap)
        : base(device, name, image, mipMap)
    {
    }

    protected override (ID3D11Texture2D, ID3D11ShaderResourceView) CreateResources(Device device, ImageInfo image, MipMapInfo mipMap, string name)
    {
        var texture = Textures.Create(device, name, image, mipMap, BindInfo.ShaderResource, ResourceInfo.Cube);
        var view = ShaderResourceViews.CreateCube(device, texture, name, image);

        return (texture, view);
    }
}
