using Mini.Engine.DirectX.Contexts.States;

namespace Mini.Engine.DirectX.Contexts;

public sealed class RasterizerContext : DeviceContextPart
{
    public RasterizerContext(DeviceContext context)
        : base(context) { }

    public void SetRasterizerState(RasterizerState state)
    {
        this.ID3D11DeviceContext.RSSetState(state.State);
    }

    public void SetScissorRect(int x, int y, int width, int height)
    {
        this.ID3D11DeviceContext.RSSetScissorRect(x, y, width, height);
    }

    public void SetViewPort(int x, int y, float width, float height)
    {
        this.ID3D11DeviceContext.RSSetViewport(x, y, width, height);
    }
}
