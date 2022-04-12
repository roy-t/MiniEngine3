using Mini.Engine.Core;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Resources;

public sealed class RWTexture2D : ITexture2D
{
    public RWTexture2D(Device device, int width, int height, Format format, bool generateMipMaps, string user, string meaning)
    {
        this.Width = width;
        this.Height = height;
        this.Format = format;

        this.MipMapSlices = generateMipMaps ? Dimensions.MipSlices(width, height) : 1;
        this.Texture = Create(device, width, height, format, generateMipMaps, user, meaning);
        this.ShaderResourceView = ShaderResourceViews.Create(device, this.Texture, format, user, meaning);
        this.UnorderedAccessViews = new ID3D11UnorderedAccessView[this.MipMapSlices];

        for(var i = 0; i < this.UnorderedAccessViews.Length; i++)
        {
            var description = new UnorderedAccessViewDescription(this.Texture, UnorderedAccessViewDimension.Texture2D, format, i, 0, 1);
            this.UnorderedAccessViews[i] = device.ID3D11Device.CreateUnorderedAccessView(this.Texture, description);
        }

        this.Name = DebugNameGenerator.GetName(user, "RWTexture2D", meaning, format);
    }

    public void SetPixels<T>(Device device, Span<T> pixels)
        where T : unmanaged
    {
        if (this.Format.IsCompressed())
        {
            throw new NotSupportedException($"Uploading data in compressed texture formats is not supported: {this.Format}");
        }

        // Assumes texture is uncompressed and fills the entire buffer
        var pitch = this.Width * this.Format.SizeOfInBytes();
        device.ID3D11DeviceContext.UpdateSubresource(pixels, this.Texture, 0, pitch, 0);

        if (this.MipMapSlices > 1)
        {
            device.ID3D11DeviceContext.GenerateMips(this.ShaderResourceView);
        }
    }

    public string Name { get; }
    public int Width { get; }
    public int Height { get; }
    public Format Format { get; }
    public int MipMapSlices { get; }

    internal ID3D11ShaderResourceView ShaderResourceView { get; }
    internal ID3D11Texture2D Texture { get; }
    internal ID3D11UnorderedAccessView[] UnorderedAccessViews { get; }

    ID3D11ShaderResourceView ITexture.ShaderResourceView => this.ShaderResourceView;
    ID3D11Texture2D ITexture.Texture => this.Texture;

    private static ID3D11Texture2D Create(Device device, int width, int height, Format format, bool generateMipMaps, string user, string meaning)
    {
        var description = new Texture2DDescription
        {
            Width = width,
            Height = height,
            MipLevels = generateMipMaps ? 0 : 1,
            ArraySize = 1,
            Format = format,
            SampleDescription = new SampleDescription(1, 0),
            Usage = ResourceUsage.Default,
            BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget | BindFlags.UnorderedAccess,
            CpuAccessFlags = CpuAccessFlags.None,
            OptionFlags = (generateMipMaps ? ResourceOptionFlags.GenerateMips : ResourceOptionFlags.None)
        };

        var texture = device.ID3D11Device.CreateTexture2D(description);
        texture.DebugName = DebugNameGenerator.GetName(user, "Texture2D", meaning, format); ;

        return texture;
    }

    public void Dispose()
    {
        this.ShaderResourceView.Dispose();
        this.Texture.Dispose();
    }
}
