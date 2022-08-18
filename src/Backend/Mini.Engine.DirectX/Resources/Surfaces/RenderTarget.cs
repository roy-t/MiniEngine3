using Mini.Engine.Core;
using Vortice.Direct3D11;

namespace Mini.Engine.DirectX.Resources.Surfaces;
public class RenderTarget : Surface, IRenderTarget
{
    public RenderTarget(Device device, string name, ImageInfo image, MipMapInfo mipMap)
        : base(name, image)
    {
        this.MipMapLevels = mipMap.Levels;

        var (texture, view) = this.CreateResources(device, name, image, mipMap);

        this.SetResources(texture, view);

        var rtvs = new ID3D11RenderTargetView[image.ArraySize * mipMap.Levels];
        for (var i = 0; i < image.ArraySize; i++)
        {
            for (var s = 0; s < mipMap.Levels; s++)
            {
                var index = Indexes.ToOneDimensional(i, s, image.ArraySize);
                rtvs[index] = RenderTargetViews.Create(device, texture, image.Format, i, s, name, "");
            }
        }

        this.AsRenderTarget.ID3D11RenderTargetViews = rtvs;
    }

    protected virtual (ID3D11Texture2D, ID3D11ShaderResourceView) CreateResources(Device device, string name, ImageInfo image, MipMapInfo mipMap)
    {
        var texture = Textures.Create(name, "", device, image, mipMap, BindInfo.RenderTarget);
        var view = ShaderResourceViews.Create(device, texture, image, name);


        return (texture, view);
    }

    public int MipMapLevels { get; }

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
