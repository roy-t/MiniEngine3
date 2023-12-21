using LibGame.Mathematics;
using Vortice.Direct3D11;

namespace Mini.Engine.DirectX.Resources.Surfaces;
public class RenderTarget : Surface, IRenderTarget
{
    public RenderTarget(Device device, string name, ImageInfo image, MipMapInfo mipMap, bool enableMSAA = false)
        : base(name, image, mipMap)
    {
        var sampling = SamplingInfo.None;
        if(enableMSAA)
        {
            sampling = SamplingInfo.GetMaximum(device, image.Format);
        }
        var (texture, view) = this.CreateResources(device, name, image, mipMap, sampling);

        this.texture = texture;
        this.shaderResourceView = view;

        var rtvs = new ID3D11RenderTargetView[image.DimZ * mipMap.Levels];
        for (var i = 0; i < image.DimZ; i++)
        {
            for (var s = 0; s < mipMap.Levels; s++)
            {
                var index = Indexes.ToOneDimensional(i, s, image.DimZ);
                rtvs[index] = RenderTargetViews.Create(device, texture, name, image.Format, sampling, i, s);
            }
        }

        this.AsRenderTarget.ID3D11RenderTargetViews = rtvs;
    }

    protected virtual (ID3D11Texture2D, ID3D11ShaderResourceView) CreateResources(Device device, string name, ImageInfo image, MipMapInfo mipMap, SamplingInfo sampling)
    {
        var texture = Textures.Create(device, name, image, mipMap, BindInfo.RenderTarget, sampling, ResourceInfo.Texture);
        var view = ShaderResourceViews.Create(device, texture, name, image, sampling);


        return (texture, view);
    }

    public IRenderTarget AsRenderTarget => this;

#nullable disable
    ID3D11RenderTargetView[] IRenderTarget.ID3D11RenderTargetViews { get; set; }
#nullable restore

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        for (var i = 0; i < this.AsRenderTarget.ID3D11RenderTargetViews.Length; i++)
        {
            this.AsRenderTarget.ID3D11RenderTargetViews[i].Dispose();
        }
    }
}
