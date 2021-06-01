using System;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX
{
    public sealed class Texture2D : IDisposable
    {
        public unsafe Texture2D(ID3D11Device device, ID3D11DeviceContext context, Span<byte> pixels, int width, int height, Format format, bool generateMipMaps = false, string name = "")
        {
            if (format.IsCompressed())
            {
                throw new NotSupportedException($"Compressed texture formats are not supported: {format}");
            }

            var description = new Texture2DDescription
            {
                Width = width,
                Height = height,
                MipLevels = generateMipMaps ? 0 : 1,
                ArraySize = 1,
                Format = format,
                SampleDescription = new SampleDescription(1, 0),
                Usage = Vortice.Direct3D11.Usage.Default,
                BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = generateMipMaps ? ResourceOptionFlags.GenerateMips : ResourceOptionFlags.None
            };

            // Assumes texture is uncompressed and fills the entire buffer
            var pitch = width * format.SizeOfInBytes();

            var view = new ShaderResourceViewDescription
            {
                Format = format,
                ViewDimension = ShaderResourceViewDimension.Texture2D,
                Texture2D = new Texture2DShaderResourceView { MipLevels = -1 }
            };

            var texture = device.CreateTexture2D(description);
            texture.DebugName = name;

            this.ShaderResourceView = device.CreateShaderResourceView(texture, view);
            context.UpdateSubresource(pixels, texture, 0, pitch, 0);

            texture.Release();
        }

        public ID3D11ShaderResourceView ShaderResourceView { get; }

        public void Dispose() =>
            this.ShaderResourceView.Release();
    }
}
