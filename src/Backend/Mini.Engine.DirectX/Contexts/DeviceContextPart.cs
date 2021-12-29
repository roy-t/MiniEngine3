using Vortice.Direct3D11;

namespace Mini.Engine.DirectX.Contexts;

public abstract class DeviceContextPart
{
    protected readonly DeviceContext DeviceContext;
    protected readonly ID3D11DeviceContext ID3D11DeviceContext;

    protected DeviceContextPart(DeviceContext context)
    {
        this.DeviceContext = context;
        this.ID3D11DeviceContext = context.ID3D11DeviceContext;
    }
}
