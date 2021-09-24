using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;
using static Vortice.Direct3D11.D3D11;
using System.Runtime.CompilerServices;
using Vortice.Direct3D11.Debug;
using System;

[assembly: InternalsVisibleTo("Mini.Engine.Debugging")]

namespace Mini.Engine.DirectX
{
    public sealed class Device : IDisposable
    {
        private readonly IntPtr WindowHandle;
        private readonly Format Format;

        private IDXGISwapChain swapChain = null!;

#if DEBUG
        private static readonly DeviceCreationFlags Flags = DeviceCreationFlags.Debug;
#else
        private static readonly DeviceCreationFlags Flags = DeviceCreationFlags.None;
#endif

        public Device(IntPtr windowHandle, Format format, int width, int height, string name)
        {
            this.WindowHandle = windowHandle;
            this.Format = format;
            this.Width = width;
            this.Height = height;

#nullable disable
            _ = D3D11CreateDevice(null, DriverType.Hardware, Flags, null, out var device, out var context);
#nullable restore
            this.ID3D11Device = device;
            this.ID3D11Device.SetName(name);
#if DEBUG
            this.ID3D11Debug = device.QueryInterface<ID3D11Debug>();
#endif
            this.ID3D11DeviceContext = context;

            this.CreateSwapChain(width, height);

            this.ImmediateContext = new ImmediateDeviceContext(this, context, "ImmediateDeviceContext");

            this.SamplerStates = new SamplerStates(device);
            this.BlendStates = new BlendStates(device);
            this.DepthStencilStates = new DepthStencilStates(device);
            this.RasterizerStates = new RasterizerStates(device);
        }

        public ImmediateDeviceContext ImmediateContext { get; }

        public int Width { get; private set; }
        public int Height { get; private set; }

        public SamplerStates SamplerStates { get; }
        public BlendStates BlendStates { get; }
        public DepthStencilStates DepthStencilStates { get; }
        public RasterizerStates RasterizerStates { get; }

        internal ID3D11Device ID3D11Device { get; }
#if DEBUG
        internal ID3D11Debug ID3D11Debug { get; }
#endif
        internal ID3D11DeviceContext ID3D11DeviceContext { get; }

        internal ID3D11Texture2D BackBuffer { get; private set; } = null!;
        internal ID3D11RenderTargetView BackBufferView { get; private set; } = null!;

        public DeferredDeviceContext CreateDeferredContextFor<T>()
        {
            return new(this, this.ID3D11Device.CreateDeferredContext(), $"{typeof(T).Name}DeferredContext");
        }

        public void Clear(RenderTarget2D renderTarget, Color4 color)
        {
            var dc = this.ID3D11DeviceContext;
            dc.ClearRenderTargetView(renderTarget.ID3D11RenderTargetView, color);
        }

        public void Clear(DepthStencilBuffer depthStencilBuffer, DepthStencilClearFlags flags, float depth, byte stencil)
        {
            var dc = this.ID3D11DeviceContext;
            dc.ClearDepthStencilView(depthStencilBuffer.DepthStencilView, flags, depth, stencil);
        }

        public void ClearBackBuffer()
        {
            var dc = this.ID3D11DeviceContext;

            dc.ClearRenderTargetView(this.BackBufferView, new Color4(0, 0, 0));
            dc.OMSetRenderTargets(this.BackBufferView);
            dc.RSSetViewport(0, 0, this.Width, this.Height);
        }

        public void Present()
        {
            this.swapChain.Present(0, PresentFlags.AllowTearing);
        }

        public void Resize(int width, int height)
        {
            this.Width = width;
            this.Height = height;

            this.BackBufferView.Dispose();
            this.BackBuffer.Dispose();

            var swapChainDescription = this.CreateSwapChainDescription(width, height);
            this.swapChain.ResizeBuffers(swapChainDescription.BufferCount,
                swapChainDescription.Width,
                swapChainDescription.Height,
                swapChainDescription.Format,
                swapChainDescription.Flags);

            this.BackBuffer = this.swapChain.GetBuffer<ID3D11Texture2D1>(0);
            this.BackBufferView = this.ID3D11Device.CreateRenderTargetView(this.BackBuffer);
            this.BackBufferView.DebugName = "BackBufferView";
        }

        private void CreateSwapChain(int width, int height)
        {
            var dxgiFactory = this.ID3D11Device.QueryInterface<IDXGIDevice>()
                ?.GetParent<IDXGIAdapter>()
                ?.GetParent<IDXGIFactory5>() // Requires DXGI 1.5 which was added in the Windows 10 Anniverary edition
                ?? throw new Exception("Could not query for IDXGIAdapter or IDXGIFactory");

            var swapchainDesc = CreateSwapChainDescription(width, height);

            this.swapChain = dxgiFactory.CreateSwapChainForHwnd(this.ID3D11Device, this.WindowHandle, swapchainDesc);

            this.BackBuffer = this.swapChain.GetBuffer<ID3D11Texture2D>(0);
            this.BackBufferView = this.ID3D11Device.CreateRenderTargetView(this.BackBuffer);
            this.BackBufferView.DebugName = "BackBufferView";
        }

        private SwapChainDescription1 CreateSwapChainDescription(int width, int height)
        {
            var dxgiFactory = this.ID3D11Device.QueryInterface<IDXGIDevice>()
               ?.GetParent<IDXGIAdapter>()
               ?.GetParent<IDXGIFactory5>() // Requires DXGI 1.5 which was added in the Windows 10 Anniverary edition
               ?? throw new Exception("Could not query for IDXGIAdapter or IDXGIFactory");

            return new SwapChainDescription1()
            {
                BufferCount = 3,
                Format = this.Format,
                AlphaMode = AlphaMode.Unspecified,
                Height = height,
                Width = width,
                Scaling = Scaling.None,
                Stereo = false,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.FlipSequential,
                Usage = Usage.RenderTargetOutput,
                Flags = dxgiFactory.PresentAllowTearing ? SwapChainFlags.AllowTearing : SwapChainFlags.None
            };
        }

        public void Dispose()
        {
            this.ID3D11DeviceContext.ClearState();
            this.ID3D11DeviceContext.Flush();

            this.BackBufferView?.Dispose();
            this.ImmediateContext?.Dispose();
            this.SamplerStates?.Dispose();
            this.BlendStates?.Dispose();
            this.DepthStencilStates?.Dispose();
            this.RasterizerStates?.Dispose();

            this.BackBuffer?.Dispose();
            this.swapChain?.Dispose();

            this.ID3D11DeviceContext.Dispose();
#if DEBUG
            this.ID3D11Debug.ReportLiveDeviceObjects(ReportLiveDeviceObjectFlags.Detail);
#endif
            this.ID3D11Device.Dispose();
        }
    }
}
