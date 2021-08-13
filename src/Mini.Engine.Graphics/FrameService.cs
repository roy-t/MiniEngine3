using Mini.Engine.Configuration;
using Mini.Engine.DirectX;

namespace Mini.Engine.Graphics;

[Service]
public class FrameService
{
    public FrameService(Device device) => this.GBuffer = new GBuffer(device);

    public GBuffer GBuffer { get; }
}
