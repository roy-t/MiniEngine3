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

    public void SetRenderTarget(RenderTarget2D renderTarget, DepthStencilBuffer? depthStencilBuffer = null)
    {
        this.ID3D11DeviceContext.OMSetRenderTargets(renderTarget.ID3D11RenderTargetView, depthStencilBuffer?.DepthStencilView);
    }

    public void SetRenderTarget(RenderTargetCube renderTarget, CubeMapFace face, DepthStencilBuffer? depthStencilBuffer = null)
    {
        this.ID3D11DeviceContext.OMSetRenderTargets(renderTarget.FaceRenderTargetViews[(int)face], depthStencilBuffer?.DepthStencilView);
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
