using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;
using static Vortice.Direct3D11.D3D11;

namespace Mini.Engine.Windows
{
    public abstract class DirectXWindow : Win32Window
    {
        private readonly Format Format = Format.R8G8B8A8_UNorm;

        private IDXGISwapChain swapChain;
        private ID3D11Texture2D backBuffer;
        private ID3D11RenderTargetView renderView;

        public DirectXWindow(string title, int width, int height)
            : base(title, width, height)
        {
            D3D11CreateDevice(null, DriverType.Hardware, DeviceCreationFlags.None, null, out var device, out var deviceContext);
            this.Device = device;
            this.DeviceContext = deviceContext;
        }

        public ID3D11Device Device { get; }
        public ID3D11DeviceContext DeviceContext { get; }

        public void Frame()
        {
            this.Clear();
            this.Render();
            this.Present();
        }

        private void Clear()
        {
            var dc = this.DeviceContext;
            dc.ClearRenderTargetView(this.renderView, new Color4(0, 0, 0));
            dc.OMSetRenderTargets(this.renderView);
            dc.RSSetViewport(0, 0, this.Width, this.Height);
        }

        private void Present()
            => this.swapChain.Present(0, PresentFlags.None);

        protected abstract void Render();

        protected override void Resize()
        {
            if (this.renderView == null) //first show
            {
                var dxgiFactory = this.Device.QueryInterface<IDXGIDevice>().GetParent<IDXGIAdapter>().GetParent<IDXGIFactory>();

                var swapchainDesc = new SwapChainDescription()
                {
                    BufferCount = 1,
                    BufferDescription = new ModeDescription(this.Width, this.Height, this.Format),
                    IsWindowed = true,
                    OutputWindow = this.Handle,
                    SampleDescription = new SampleDescription(1, 0),
                    SwapEffect = SwapEffect.Discard,
                    Usage = Vortice.DXGI.Usage.RenderTargetOutput
                };

                this.swapChain = dxgiFactory.CreateSwapChain(this.Device, swapchainDesc);
                dxgiFactory.MakeWindowAssociation(this.Handle, WindowAssociationFlags.IgnoreAll);

                this.backBuffer = this.swapChain.GetBuffer<ID3D11Texture2D>(0);
                this.renderView = this.Device.CreateRenderTargetView(this.backBuffer);
            }
            else
            {
                this.renderView.Dispose();
                this.backBuffer.Dispose();

                this.swapChain.ResizeBuffers(1, this.Width, this.Height, this.Format, SwapChainFlags.None);

                this.backBuffer = this.swapChain.GetBuffer<ID3D11Texture2D1>(0);
                this.renderView = this.Device.CreateRenderTargetView(this.backBuffer);
            }
        }

        public override void Dispose()
        {
            this.renderView?.Dispose();
            this.backBuffer?.Dispose();
            this.swapChain?.Dispose();


            this.Device.Dispose();
            this.DeviceContext.Dispose();

            base.Dispose();
        }
    }
}
