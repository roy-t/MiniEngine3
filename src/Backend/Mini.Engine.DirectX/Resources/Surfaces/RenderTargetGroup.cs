using Vortice.Direct3D11;

namespace Mini.Engine.DirectX.Resources.Surfaces;
public sealed class RenderTargetGroup : IDisposable
{
    public RenderTargetGroup(params IRenderTarget[] renderTargets)
    {
        this.Views = new ID3D11RenderTargetView[renderTargets.Length];
        for (var i = 0; i < renderTargets.Length; i++)
        {
            this.Views[i] = renderTargets[i].ID3D11RenderTargetViews[0];
        }
    }

    internal ID3D11RenderTargetView[] Views { get; }

    public void Dispose()
    {
        for (var i = 0; i < this.Views.Length; i++)
        {
            this.Views[i].Dispose();
        }
    }
}
