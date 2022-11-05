using Vortice.Direct3D11;

namespace Mini.Engine.DirectX.Resources.Shaders;

public interface IPixelShader : IShader
{
    internal ID3D11PixelShader ID3D11Shader { get; }
}

public sealed class PixelShader : IPixelShader
{
    private readonly ID3D11PixelShader Shader;

    public PixelShader(Device device, string name, byte[] byteCode)
    {
        this.Shader = device.ID3D11Device.CreatePixelShader(byteCode);
        this.Shader.DebugName = this.Name = name;
    }

    public string Name { get; }

    ID3D11PixelShader IPixelShader.ID3D11Shader => this.Shader;

    public void Dispose()
    {
        this.Shader.Dispose();
    }    
}
