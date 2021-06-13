﻿using System;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;
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
            this.Width = width;
            this.Height = height;

            D3D11CreateDevice(null, DriverType.Hardware, DeviceCreationFlags.None, null, out var device, out var context);

            this.GraphicsDevice = device;
            this.ImmediateContext = context;

            device.AddRef(); device.AddRef(); device.AddRef(); device.AddRef(); device.AddRef(); device.AddRef();

            this.CreateSwapChain(width, height);

            this.SamplerStates = new SamplerStates(this.GraphicsDevice);
        }

        public ID3D11Device GetDevice() => this.GraphicsDevice;
        public ID3D11DeviceContext GetImmediateContext() => this.ImmediateContext;
        public ID3D11RenderTargetView GetBackBufferView() => this.renderView;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public SamplerStates SamplerStates { get; }

        public void Clear()
        {
            var dc = this.ImmediateContext;

            dc.ClearRenderTargetView(this.renderView, new Color4(0, 0, 0));
            dc.OMSetRenderTargets(this.renderView);
            dc.RSSetViewport(0, 0, this.Width, this.Height);
        }

        public void Present() => this.swapChain.Present(0, PresentFlags.None);

        public void Resize(int width, int height)
        {
            this.Width = width;
            this.Height = height;

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
