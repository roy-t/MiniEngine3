﻿using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX
{
    public sealed class RenderTarget2D : Texture2D
    {
        public RenderTarget2D(Device device, int width, int height, Format format, bool generateMipMaps, string name)
            : base(device, width, height, format, generateMipMaps, name)
            => this.ID3D11RenderTargetView = device.ID3D11Device.CreateRenderTargetView(this.Texture);

        internal ID3D11RenderTargetView ID3D11RenderTargetView { get; }

        public override void Dispose()
        {
            this.ID3D11RenderTargetView.Dispose();
            base.Dispose();
        }
    }
}