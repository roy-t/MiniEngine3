using System;
using Vortice.Direct3D11;

namespace Mini.Engine.DirectX
{
    public sealed class RenderTarget2D : IDisposable
    {
        internal RenderTarget2D(ID3D11RenderTargetView iD3D11RenderTargetView, string name)
        {
            this.ID3D11RenderTargetView = iD3D11RenderTargetView;
            this.ID3D11RenderTargetView.DebugName = name;
        }

        internal ID3D11RenderTargetView ID3D11RenderTargetView { get; }

        public void Dispose()
            => this.ID3D11RenderTargetView.Dispose();
    }
}
