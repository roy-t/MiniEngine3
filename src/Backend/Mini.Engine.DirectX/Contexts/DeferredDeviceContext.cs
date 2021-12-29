using Vortice.Direct3D11;

namespace Mini.Engine.DirectX.Contexts;

public sealed class DeferredDeviceContext : DeviceContext
{
    public DeferredDeviceContext(Device device, ID3D11DeviceContext context, string name)
        : base(device, context, name) { }

    public CommandList FinishCommandList()
    {
        return new(this.ID3D11DeviceContext.FinishCommandList(false));
    }
}
