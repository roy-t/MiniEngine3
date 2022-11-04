using Mini.Engine.DirectX.Buffers;
using Vortice.Direct3D11;

namespace Mini.Engine.DirectX.Resources.Shaders;

public interface IVertexShader : IShader
{
    internal ID3D11VertexShader ID3D11Shader { get; }
}

public sealed class VertexShader : IVertexShader
{
    private readonly byte[] ByteCode;
    private readonly ID3D11VertexShader Shader;

    public VertexShader(Device device, byte[] byteCode)
    {
        this.Shader = device.ID3D11Device.CreateVertexShader(byteCode);
        this.ByteCode = byteCode;
    }

    ID3D11VertexShader IVertexShader.ID3D11Shader => this.Shader;

    public InputLayout CreateInputLayout(Device device, params InputElementDescription[] elements)
    {
        return new(device.ID3D11Device.CreateInputLayout(elements, this.ByteCode));
    }

    public void Dispose()
    {
        this.Shader.Dispose();
    }
}
