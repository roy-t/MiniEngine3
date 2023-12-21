using Vortice.Direct3D11;

namespace Mini.Engine.DirectX.Resources.Surfaces;
public sealed class RenderTargetCube : RenderTarget, IRenderTargetCube
{
    public RenderTargetCube(Device device, string name, ImageInfo image, MipMapInfo mipMap)
        : base(device, name, image, mipMap)
    {
    }

    protected override (ID3D11Texture2D, ID3D11ShaderResourceView) CreateResources(Device device, string name, ImageInfo image, MipMapInfo mipMap, SamplingInfo sampling)
    {
        if (sampling.Count > 1)
        {
            throw new NotSupportedException($"Sampling count of {sampling.Count} is more than 1, which is not supported for cube textures");
        }
        var texture = Textures.Create(device, name, image, mipMap, BindInfo.RenderTarget, sampling, ResourceInfo.Cube);
        var view = ShaderResourceViews.CreateCube(device, texture, this.Name, image);

        return (texture, view);
    }
}
