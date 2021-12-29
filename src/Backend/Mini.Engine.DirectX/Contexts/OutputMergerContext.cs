using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Contexts.States;
using Mini.Engine.DirectX.Resources;

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
}
