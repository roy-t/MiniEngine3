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

    public void SetShader(IVertexShader shader)
    {
        this.ID3D11DeviceContext.VSSetShader(shader.ID3D11Shader);
    }

    public void SetInstanceBuffer<T>(int slot, IResource<StructuredBuffer<T>> instanceBuffer)
        where T : unmanaged
    {
        var resource = this.DeviceContext.Resources.Get(instanceBuffer);
        this.ID3D11DeviceContext.VSSetShaderResource(slot, resource.GetShaderResourceView());
    }
}
