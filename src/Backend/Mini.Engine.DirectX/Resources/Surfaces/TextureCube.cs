using Vortice.Direct3D11;

namespace Mini.Engine.DirectX.Resources.Surfaces;

public sealed class TextureCube : Texture, ITextureCube
{
    public TextureCube(Device device, ImageInfo image, MipMapInfo mipMap, string name)
        : base(device, image, mipMap, name)
    {
    }

    protected override (ID3D11Texture2D, ID3D11ShaderResourceView) CreateResources(Device device, ImageInfo image, MipMapInfo mipMap, string name)
    {
        var texture = Textures.Create(name, "", device, image, mipMap, BindInfo.ShaderResource, ResourceInfo.Cube);
        var view = ShaderResourceViews.CreateCube(device, texture, image, name);

        return (texture, view);
    }
}
