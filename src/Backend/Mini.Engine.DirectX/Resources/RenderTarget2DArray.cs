using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Resources;
public sealed class RenderTarget2DArray : ITexture2D
{
    // TODO: this is very similar to RenderTargetCube
    public RenderTarget2DArray(Device device, int width, int height, int length, Format format, string user, string meaning)
    {
        this.Width = width;
        this.Height = height;
        this.Length = length;
        this.Format = format;
        this.Name = DebugNameGenerator.GetName(user, "RT", meaning);

        this.Texture = Textures.Create(device, width, height, format, length, false, user, meaning);
        this.ShaderResourceView = CreateSRV(device, this.Texture, length, format, user, meaning);

        this.ID3D11RenderTargetViews = new ID3D11RenderTargetView[this.Length];
        for (var i = 0; i < length; i++)
        {
            this.ID3D11RenderTargetViews[i] = RenderTargetViews.Create(device, this.Texture, format, i, user, meaning);
        }
    }

    private static ID3D11ShaderResourceView CreateSRV(Device device, ID3D11Texture2D texture, int length, Format format, string user, string meaning)
    {
        var description = new ShaderResourceViewDescription(texture, ShaderResourceViewDimension.Texture2DArray, format, 0, -1, 0, length);
        var srv = device.ID3D11Device.CreateShaderResourceView(texture, description);
        srv.DebugName = DebugNameGenerator.GetName(user, "SRV", meaning);

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
