using Mini.Engine.DirectX;
using Vortice.DXGI;

namespace Mini.Engine.Graphics
{
    public sealed class GBuffer
    {
        public GBuffer(Device device, DepthStencilFormat depthStencilFormat)
        {
            this.Albedo = new RenderTarget2D(device, device.Width, device.Height, Format.B8G8R8A8_UNorm_SRgb, false, "GBuffer_Albedo");
            this.DepthStencilBuffer = new DepthStencilBuffer(device, depthStencilFormat, device.Width, device.Height);
        }

        public RenderTarget2D Albedo { get; }


        public DepthStencilBuffer DepthStencilBuffer { get; }
    }
}
