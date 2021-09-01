using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX
{
    public sealed class DepthStencilBuffer : IDisposable
    {
        public DepthStencilBuffer(Device device, int width, int height)
        {
            var description = new Texture2DDescription
            {
                Width = width,
                Height = height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.D24_UNorm_S8_UInt,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            };

            var view = new DepthStencilViewDescription
            {
                Format = Format.D24_UNorm_S8_UInt,
                ViewDimension = DepthStencilViewDimension.Texture2D,
                Texture2D = new Texture2DDepthStencilView() { MipSlice = 0 }
            };

            this.Texture = device.ID3D11Device.CreateTexture2D(description);
            this.Texture.DebugName = nameof(DepthStencilBuffer);

            this.DepthStencilView = device.ID3D11Device.CreateDepthStencilView(this.Texture, view);
        }

        internal ID3D11Texture2D Texture { get; }
        internal ID3D11DepthStencilView DepthStencilView { get; }

        public void Dispose()
        {
            this.DepthStencilView.Dispose();
            this.Texture.Dispose();
        }
    }
}
