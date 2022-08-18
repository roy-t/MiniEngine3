using Vortice.Direct3D11;

namespace Mini.Engine.DirectX.Resources.Surfaces;
public sealed class RenderTargetCube : RenderTarget, IRenderTargetCube
{
    public RenderTargetCube(Device device, string name, ImageInfo image, MipMapInfo mipMap)
        : base(device, name, image, mipMap)
    {
    }

    protected override (ID3D11Texture2D, ID3D11ShaderResourceView) CreateResources(Device device, string name, ImageInfo image, MipMapInfo mipMap)
    {
        var texture = Textures.Create(name, "", device, image, mipMap, BindInfo.RenderTarget, ResourceInfo.Cube);
        var view = ShaderResourceViews.CreateCube(device, texture, image, Name);

        return (texture, view);
    }
}
