using Mini.Engine.Core;
using Vortice.Direct3D11;

namespace Mini.Engine.DirectX.Resources.Surfaces;
public class RenderTarget : Surface, IRenderTarget
{
    public RenderTarget(Device device, string name, ImageInfo image, MipMapInfo mipMap)
        : base(name, image, mipMap)
    {
        var (texture, view) = this.CreateResources(device, name, image, mipMap);

        this.SetResources(texture, view);

        var rtvs = new ID3D11RenderTargetView[image.DimZ * mipMap.Levels];
        for (var i = 0; i < image.DimZ; i++)
        {
            for (var s = 0; s < mipMap.Levels; s++)
            {
                var index = Indexes.ToOneDimensional(i, s, image.DimZ);
                rtvs[index] = RenderTargetViews.Create(device, texture, name, image.Format, i, s);
            }
        }

        this.AsRenderTarget.ID3D11RenderTargetViews = rtvs;
    }

    protected virtual (ID3D11Texture2D, ID3D11ShaderResourceView) CreateResources(Device device, string name, ImageInfo image, MipMapInfo mipMap)
    {
        var texture = Textures.Create(device, name, image, mipMap, BindInfo.RenderTarget);
        var view = ShaderResourceViews.Create(device, texture, name, image);


        return (texture, view);
    }

    public IRenderTarget AsRenderTarget => this;

#nullable disable
    ID3D11RenderTargetView[] IRenderTarget.ID3D11RenderTargetViews { get; set; }
#nullable restore

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        for (var i = 0; i < this.DimZ; i++)
        {
            this.AsRenderTarget.ID3D11RenderTargetViews[i].Dispose();
        }
    }
}
