using Vortice.Direct3D11;

namespace Mini.Engine.DirectX.Contexts;

public sealed class ImmediateDeviceContext : DeviceContext
{
    public ImmediateDeviceContext(Device device, ID3D11DeviceContext context, string user)
        : base(device, context, user, nameof(ImmediateDeviceContext)) { }

    public void ExecuteCommandList(CommandList commandList)
    {
        this.ID3D11DeviceContext.ExecuteCommandList(commandList.ID3D11CommandList, false);
    }
}
