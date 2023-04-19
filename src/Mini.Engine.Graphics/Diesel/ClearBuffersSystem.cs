using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Surfaces;
using Vortice.Direct3D11;
using Vortice.Mathematics;

namespace Mini.Engine.Graphics.Diesel;

public static class ClearBuffersSystem
{
    public static void Clear(Device device, RenderTarget renderTarget, Color4 clearColor)
    {
        var context = device.ImmediateContext;

        context.Clear(renderTarget, clearColor);
    }

    public static void Clear(Device device, DepthStencilBuffer depthStencilBuffer)
    {
        var context = device.ImmediateContext;

        context.Clear(depthStencilBuffer, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 0.0f, 0);
    }
}
