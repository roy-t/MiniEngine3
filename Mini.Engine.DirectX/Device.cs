using System;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using static Vortice.Direct3D11.D3D11;

namespace Mini.Engine.DirectX
{
    public sealed class Device : IDisposable
    {
        private readonly ID3D11Device GraphicsDevice;
        private readonly ID3D11DeviceContext ImmediateContext;
        private readonly IntPtr WindowHandle;
        private readonly Format Format;

        private IDXGISwapChain swapChain;
        private ID3D11Texture2D backBuffer;
        private ID3D11RenderTargetView renderView;

        public Device(IntPtr windowHandle, Format format, int width, int height)
        {
            this.WindowHandle = windowHandle;
            this.Format = format;

            var flags = DeviceCreationFlags.None;
#if DEBUG
            flags |= DeviceCreationFlags.Debug;
#endif

            D3D11CreateDevice(null, DriverType.Hardware, flags, null, out var device, out var context);
            this.GraphicsDevice = device;
            this.ImmediateContext = context;

            this.CreateSwapChain(width, height);
        }

        public ID3D11Device GetDevice() => this.GraphicsDevice;
        public ID3D11DeviceContext GetImmediateContext => this.ImmediateContext;

        public void Resize(int width, int height)
        {
            this.renderView.Dispose();
            this.backBuffer.Dispose();

            this.swapChain.ResizeBuffers(1, width, height, this.Format, SwapChainFlags.None);

            this.backBuffer = this.swapChain.GetBuffer<ID3D11Texture2D1>(0);
            this.renderView = this.GraphicsDevice.CreateRenderTargetView(this.backBuffer);
        }

        private void CreateSwapChain(int width, int height)
        {
            var dxgiFactory = this.GraphicsDevice.QueryInterface<IDXGIDevice>().GetParent<IDXGIAdapter>().GetParent<IDXGIFactory>();

            var swapchainDesc = new SwapChainDescription()
            {
                BufferCount = 1,
                BufferDescription = new ModeDescription(width, height, this.Format),
                IsWindowed = true,
                OutputWindow = this.WindowHandle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Vortice.DXGI.Usage.RenderTargetOutput
            };

            this.swapChain = dxgiFactory.CreateSwapChain(this.GraphicsDevice, swapchainDesc);
            dxgiFactory.MakeWindowAssociation(this.WindowHandle, WindowAssociationFlags.IgnoreAll);

            this.backBuffer = this.swapChain.GetBuffer<ID3D11Texture2D>(0);
            this.renderView = this.GraphicsDevice.CreateRenderTargetView(this.backBuffer);
        }

        public void Dispose()
        {
            this.renderView?.Dispose();
            this.backBuffer?.Dispose();
            this.swapChain?.Dispose();

            this.ImmediateContext.ClearState();
            this.ImmediateContext.Flush();
            this.ImmediateContext.Dispose();

            this.GraphicsDevice.Dispose();
        }
    }
}
