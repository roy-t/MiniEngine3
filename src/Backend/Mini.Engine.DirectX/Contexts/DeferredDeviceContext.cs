using Vortice.Direct3D11;

namespace Mini.Engine.DirectX.Contexts;

public sealed class DeferredDeviceContext : DeviceContext
{
    private readonly string User;

    public DeferredDeviceContext(Device device, ID3D11DeviceContext context, ResourceManager resources, string user)
        : base(device, context, resources, user, nameof(DeferredDeviceContext))
    {
        this.User = user;
    }

    public CommandList FinishCommandList()
    {
        return new(this.ID3D11DeviceContext.FinishCommandList(false), this.User);
    }
}
