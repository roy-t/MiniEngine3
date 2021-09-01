using Mini.Engine.Configuration;
using Mini.Engine.DirectX;

namespace Mini.Engine.Graphics;

[Service]
public class FrameService
{
    public FrameService(Device device)
    {
        // TODO: try a 32 bit floating point depth buffer
        this.GBuffer = new GBuffer(device, DepthStencilFormat.D24_UNorm_S8_UInt);
    }

    public GBuffer GBuffer { get; }
}
