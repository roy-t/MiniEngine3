using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Resources.Shaders;

namespace Mini.Engine.DirectX.Contexts;

public sealed class VertexShaderContext : DeviceContextPart
{
    public VertexShaderContext(DeviceContext context)
        : base(context) { }

    public void SetConstantBuffer<T>(int slot, ConstantBuffer<T> buffer)
        where T : unmanaged
    {
        this.ID3D11DeviceContext.VSSetConstantBuffer(slot, buffer.Buffer);
    }

    public void SetShader(ILifetime<IVertexShader> shader)
    {
        var resource = this.DeviceContext.Resources.Get(shader).ID3D11Shader;
        this.ID3D11DeviceContext.VSSetShader(resource);
    }

    public void SetInstanceBuffer<T>(int slot, ILifetime<StructuredBuffer<T>> instanceBuffer)
        where T : unmanaged
    {
        var resource = this.DeviceContext.Resources.Get(instanceBuffer);
        this.ID3D11DeviceContext.VSSetShaderResource(slot, resource.GetShaderResourceView());
    }
}
