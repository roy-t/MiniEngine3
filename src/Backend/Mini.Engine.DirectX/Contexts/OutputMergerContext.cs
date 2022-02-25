using Mini.Engine.Core;
using Mini.Engine.DirectX.Contexts.States;
using Mini.Engine.DirectX.Resources;
using Vortice.Direct3D11;

namespace Mini.Engine.DirectX.Contexts;

public sealed class OutputMergerContext : DeviceContextPart
{
    public OutputMergerContext(DeviceContext context)
        : base(context) { }

    public void SetBlendState(BlendState state)
    {
        this.ID3D11DeviceContext.OMSetBlendState(state.ID3D11BlendState);
    }

    public void SetDepthStencilState(DepthStencilState state)
    {
        this.ID3D11DeviceContext.OMSetDepthStencilState(state.ID3D11DepthStencilState);
    }

    public void SetRenderTargetToBackBuffer(DepthStencilBuffer? depthStencilBuffer = null)
    {
        this.ID3D11DeviceContext.OMSetRenderTargets(base.DeviceContext.Device.BackBufferView, depthStencilBuffer?.DepthStencilView);
    }

    public void SetRenderTarget(DepthStencilBuffer depthStencilBuffer )
    {
#nullable disable
        this.ID3D11DeviceContext.OMSetRenderTargets((ID3D11RenderTargetView)null, depthStencilBuffer.DepthStencilView);
#nullable restore
    }

    public void SetRenderTarget(DepthStencilBufferArray depthStencilBuffers, int slice)
    {
#nullable disable
        this.ID3D11DeviceContext.OMSetRenderTargets((ID3D11RenderTargetView)null, depthStencilBuffers.DepthStencilViews[slice]);
#nullable restore
    }

    public void SetRenderTarget(RenderTarget2D renderTarget, DepthStencilBuffer? depthStencilBuffer = null)
    {
        this.ID3D11DeviceContext.OMSetRenderTargets(renderTarget.ID3D11RenderTargetView, depthStencilBuffer?.DepthStencilView);
    }

    public void SetRenderTarget(RenderTarget2DArray renderTarget, int slice, DepthStencilBuffer? depthStencilBuffer = null)
    {
        this.ID3D11DeviceContext.OMSetRenderTargets(renderTarget.ID3D11RenderTargetViews[slice], depthStencilBuffer?.DepthStencilView);
    }

    public void SetRenderTarget(RenderTargetCube renderTarget, CubeMapFace face, int mipSlice = 0, DepthStencilBuffer? depthStencilBuffer = null)
    {
        var slice = Indexes.ToOneDimensional(mipSlice, (int)face, 6);
        this.ID3D11DeviceContext.OMSetRenderTargets(renderTarget.FaceRenderTargetViews[slice], depthStencilBuffer?.DepthStencilView);
    }

    public void SetRenderTargets(DepthStencilBuffer? depthStencilBuffer, params RenderTarget2D[] renderTargets)
    {
        var views = new ID3D11RenderTargetView[renderTargets.Length];
        for (var i = 0; i < renderTargets.Length; i++)
        {
            views[i] = renderTargets[i].ID3D11RenderTargetView;
        }
        this.ID3D11DeviceContext.OMSetRenderTargets(views, depthStencilBuffer?.DepthStencilView);
    }
}
