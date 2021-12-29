using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Resources;

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
}
