using System;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;
using static Vortice.Direct3D11.D3D11;

namespace Mini.Engine.DirectX
{
    public sealed class Device : IDisposable
    {
        private readonly IntPtr WindowHandle;
        private readonly Format Format;

        private IDXGISwapChain swapChain;
        private ID3D11Texture2D backBuffer;

        public Device(IntPtr windowHandle, Format format, int width, int height)
        {
            this.WindowHandle = windowHandle;
            this.Format = format;
            this.Width = width;
            this.Height = height;

            D3D11CreateDevice(null, DriverType.Hardware, DeviceCreationFlags.None, null, out var device, out var context);

            this.ID3D11Device = device;
            this.ID3D11DeviceContext = context;

            this.CreateSwapChain(width, height);

            this.ImmediateContext = new ImmediateDeviceContext(context);

            this.SamplerStates = new SamplerStates(device);
            this.BlendStates = new BlendStates(device);
            this.DepthStencilStates = new DepthStencilStates(device);
            this.RasterizerStates = new RasterizerStates(device);
        }

        // TODO: temp properties
        public ID3D11Device GetDevice() => this.ID3D11Device;
        public ID3D11DeviceContext GetImmediateContext() => this.ID3D11DeviceContext;
        public RenderTarget2D BackBuffer { get; private set; }

        public ImmediateDeviceContext ImmediateContext { get; }


        internal ID3D11Device ID3D11Device { get; }
        internal ID3D11DeviceContext ID3D11DeviceContext { get; }


        public int Width { get; private set; }
        public int Height { get; private set; }

        public SamplerStates SamplerStates { get; }
        public BlendStates BlendStates { get; }
        public DepthStencilStates DepthStencilStates { get; }
        public RasterizerStates RasterizerStates { get; }

        public DeferredDeviceContext CreateDeferredContext()
            => new DeferredDeviceContext(this.ID3D11Device.CreateDeferredContext());

        public void Clear()
        {
            var dc = this.ID3D11DeviceContext;

            dc.ClearRenderTargetView(this.BackBuffer.ID3D11RenderTargetView, new Color4(0, 0, 0));
            dc.OMSetRenderTargets(this.BackBuffer.ID3D11RenderTargetView);
            dc.RSSetViewport(0, 0, this.Width, this.Height);
        }

        public void Present() => this.swapChain.Present(0, PresentFlags.None);

        public void Resize(int width, int height)
        {
            this.Width = width;
            this.Height = height;

            this.BackBuffer.Dispose();
            this.backBuffer.Dispose();

            this.swapChain.ResizeBuffers(1, width, height, this.Format, SwapChainFlags.None);

            this.backBuffer = this.swapChain.GetBuffer<ID3D11Texture2D1>(0);
            this.BackBuffer = new RenderTarget2D(this.ID3D11Device.CreateRenderTargetView(this.backBuffer), "BackBuffer");
        }

        private void CreateSwapChain(int width, int height)
        {
            var dxgiFactory = this.ID3D11Device.QueryInterface<IDXGIDevice>().GetParent<IDXGIAdapter>().GetParent<IDXGIFactory>();

            var swapchainDesc = new SwapChainDescription()
            {
                BufferCount = 1,
                BufferDescription = new ModeDescription(width, height, this.Format),
                IsWindowed = true,
                OutputWindow = this.WindowHandle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            this.swapChain = dxgiFactory.CreateSwapChain(this.ID3D11Device, swapchainDesc);
            dxgiFactory.MakeWindowAssociation(this.WindowHandle, WindowAssociationFlags.IgnoreAll);

            this.backBuffer = this.swapChain.GetBuffer<ID3D11Texture2D>(0);
            this.BackBuffer = new RenderTarget2D(this.ID3D11Device.CreateRenderTargetView(this.backBuffer), "BackBuffer");
        }

        public void Dispose()
        {
            this.BackBuffer?.Dispose();
            this.backBuffer?.Dispose();
            this.swapChain?.Dispose();

            this.ID3D11DeviceContext.ClearState();
            this.ID3D11DeviceContext.Flush();
            this.ID3D11DeviceContext.Dispose();

            this.ID3D11Device.Dispose();
        }
    }
}
