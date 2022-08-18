using Mini.Engine.Core;
using Mini.Engine.DirectX.Contexts.States;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.DirectX.Resources.Surfaces;
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

    public void SetRenderTargetToBackBuffer(IDepthStencilBuffer? depthStencilBuffer = null)
    {
        this.ID3D11DeviceContext.OMSetRenderTargets(base.DeviceContext.Device.BackBufferView, depthStencilBuffer?.DepthStencilViews[0]);
    }

    public void SetRenderTarget(IDepthStencilBuffer depthStencilBuffer)
    {
#nullable disable
        this.ID3D11DeviceContext.OMSetRenderTargets((ID3D11RenderTargetView)null, depthStencilBuffer.DepthStencilViews[0]);
#nullable restore
    }

    public void SetRenderTarget(IDepthStencilBuffer depthStencilBuffers, int slice)
    {
#nullable disable
        this.ID3D11DeviceContext.OMSetRenderTargets((ID3D11RenderTargetView)null, depthStencilBuffers.DepthStencilViews[slice]);
#nullable restore
    }

    public void SetRenderTarget(IResource<IDepthStencilBuffer> depthStencilBuffers, int slice)
    {
        var dsv = this.DeviceContext.Resources.Get(depthStencilBuffers).DepthStencilViews[slice];
#nullable disable
        this.ID3D11DeviceContext.OMSetRenderTargets((ID3D11RenderTargetView)null, dsv);
#nullable restore
    }

    public void SetRenderTarget(IRenderTarget renderTarget, IDepthStencilBuffer? depthStencilBuffer = null)
    {
        this.ID3D11DeviceContext.OMSetRenderTargets(renderTarget.ID3D11RenderTargetViews[0], depthStencilBuffer?.DepthStencilViews[0]);
    }
   
    public void SetRenderTarget(IRenderTarget renderTarget, int index, int level = 0, IDepthStencilBuffer? depthStencilBuffer = null)
    {
        var slice = Indexes.ToOneDimensional(index, level, renderTarget.DimZ);
        this.ID3D11DeviceContext.OMSetRenderTargets(renderTarget.ID3D11RenderTargetViews[slice], depthStencilBuffer?.DepthStencilViews[0]);
    }

    public void SetRenderTargets(IDepthStencilBuffer? depthStencilBuffer, params IRenderTarget[] renderTargets)
    {
        var views = new ID3D11RenderTargetView[renderTargets.Length];
        for (var i = 0; i < renderTargets.Length; i++)
        {
            views[i] = renderTargets[i].ID3D11RenderTargetViews[0];
        }
        this.ID3D11DeviceContext.OMSetRenderTargets(views, depthStencilBuffer?.DepthStencilViews[0]);
    }
}
