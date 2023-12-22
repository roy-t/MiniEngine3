using LibGame.Mathematics;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX.Contexts.States;
using Mini.Engine.DirectX.Resources.Surfaces;
using Vortice.Direct3D11;

namespace Mini.Engine.DirectX.Contexts;

public sealed class OutputMergerContext : DeviceContextPart
{
    public OutputMergerContext(DeviceContext context)
        : base(context) { }

    public void SetBlendState(BlendState state)
    {
        unsafe
        {
            this.ID3D11DeviceContext.OMSetBlendState(state.ID3D11BlendState);
        }
    }

    public void SetDepthStencilState(DepthStencilState state)
    {
        this.ID3D11DeviceContext.OMSetDepthStencilState(state.ID3D11DepthStencilState);
    }

    public void SetRenderTargetToBackBuffer(IDepthStencilBuffer? depthStencilBuffer = null)
    {
        this.ID3D11DeviceContext.OMSetRenderTargets(this.DeviceContext.Device.BackBufferView, depthStencilBuffer?.DepthStencilViews[0]);
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

    public void SetRenderTarget(ILifetime<IDepthStencilBuffer> depthStencilBuffers, int slice)
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

    public void SetRenderTargets(RenderTargetGroup renderTargets, IDepthStencilBuffer? depthStencilBuffer = null)
    {        
        this.ID3D11DeviceContext.OMSetRenderTargets(renderTargets.Views, depthStencilBuffer?.DepthStencilViews[0]);
    }    
}
