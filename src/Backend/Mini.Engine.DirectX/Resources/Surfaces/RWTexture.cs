using Vortice.Direct3D11;

namespace Mini.Engine.DirectX.Resources.Surfaces;
public sealed class RWTexture : Surface, IRWTexture
{
    public RWTexture(Device device, string name, ImageInfo image, MipMapInfo mipMap)
        : base(name, image, mipMap)
    {
        var texture = Textures.Create(device, name, image, mipMap, BindInfo.UnorderedAccessView);
        var view = ShaderResourceViews.Create(device, texture, name, image);

        this.texture = texture;
        this.shaderResourceView = view;

        var uavs = new ID3D11UnorderedAccessView[mipMap.Levels];

        for (var i = 0; i < uavs.Length; i++)
        {
            var description = new UnorderedAccessViewDescription(texture, UnorderedAccessViewDimension.Texture2D, image.Format, i, 0, 1);
            uavs[i] = device.ID3D11Device.CreateUnorderedAccessView(texture, description);
        }

        this.AsRwTexture.UnorderedAccessViews = uavs;
    }    

    public IRWTexture AsRwTexture => this;

#nullable disable    
    ID3D11UnorderedAccessView[] IRWTexture.UnorderedAccessViews { get; set; }
#nullable restore

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        for (var i = 0; i < this.DimZ; i++)
        {
            this.AsRwTexture.UnorderedAccessViews[i].Dispose();
        }
    }
}
