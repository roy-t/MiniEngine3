using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Resources;
public sealed class RenderTarget2DArray : ITexture2D
{
    // TODO: this is very similar to RenderTargetCube
    public RenderTarget2DArray(Device device, int width, int height, int length, Format format, string name)
    {
        this.Width = width;
        this.Height = height;
        this.Length = length;
        this.Format = format;
        this.Name = name;

        this.Texture = Textures.Create(device, width, height, Format, length, false, name);
        this.ShaderResourceView = CreateSRV(device, this.Texture, length, format, name);

        this.ID3D11RenderTargetViews = new ID3D11RenderTargetView[this.Length];
        for (var i = 0; i < length; i++)
        {
            this.ID3D11RenderTargetViews[i] = RenderTargetViews.Create(device, this.Texture, format, i, name);
        }
    }

    private static ID3D11ShaderResourceView CreateSRV(Device device, ID3D11Texture2D texture, int length, Format format, string name)
{
        var description = new ShaderResourceViewDescription(texture, ShaderResourceViewDimension.Texture2DArray, format, 0, -1, 0, length);
        var srv = device.ID3D11Device.CreateShaderResourceView(texture, description);
        srv.DebugName = $"{name}_SRV";

        return srv;
    }

    public int Width { get; }
    public int Height { get; }
    public int Length { get; }
    public string Name { get; }
    public Format Format { get; }
    public int MipMapSlices { get; }

    internal ID3D11ShaderResourceView ShaderResourceView { get; }
    internal ID3D11Texture2D Texture { get; }

    ID3D11ShaderResourceView ITexture.ShaderResourceView => this.ShaderResourceView;
    ID3D11Texture2D ITexture.Texture => this.Texture;

    internal ID3D11RenderTargetView[] ID3D11RenderTargetViews { get; }

    public void Dispose()
    {
        for (var i = 0; i < this.ID3D11RenderTargetViews.Length; i++)
        {
            this.ID3D11RenderTargetViews[i].Dispose();
        }
        
        this.ShaderResourceView.Dispose();
        this.Texture.Dispose();
    }
}
