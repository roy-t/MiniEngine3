using System.Drawing;
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

    public void SetScissorRect(in Rectangle rectangle)
    {
        this.ID3D11DeviceContext.RSSetScissorRect(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height);
    }

    public void SetViewport(float x, float y, float width, float height)
    {
        this.ID3D11DeviceContext.RSSetViewport(x, y, width, height);
    }

    public void SetViewport(in Rectangle rectangle, float minDepth = 0.0f, float maxDepth = 1.0f)
    {
        this.ID3D11DeviceContext.RSSetViewport(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, minDepth, maxDepth);
    }
}
