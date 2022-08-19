using Mini.Engine.DirectX.Buffers;
using Vortice.Direct3D;
using Vortice.Direct3D11;

namespace Mini.Engine.DirectX.Resources;

public interface IPixelShader : IShader
{
    internal ID3D11PixelShader ID3D11Shader { get; set; }
}

public interface IVertexShader : IShader
{
    internal ID3D11VertexShader ID3D11Shader { get; set; }
}

public interface IComputeShader : IShader
{
    internal ID3D11ComputeShader ID3D11Shader { get; set; }
    (int X, int Y, int Z) GetDispatchSize(int dimX, int dimY, int dimZ);
}

public interface IShader
{
    InputLayout CreateInputLayout(Device device, params InputElementDescription[] elements);
}

public abstract class Shader<TShader> : IDisposable
    where TShader : ID3D11DeviceChild
{
    protected readonly Device Device;
    protected Blob blob;

    public Shader(Device device)
    {
        this.Device = device;
        this.blob = null!;
        this.ID3D11Shader = null!;
    }

    public TShader ID3D11Shader { get; set; }

    public InputLayout CreateInputLayout(Device device, params InputElementDescription[] elements)
    {
        return new(device.ID3D11Device.CreateInputLayout(elements, this.blob!));
    }

    public virtual void Dispose()
    {
        this.blob?.Dispose();
        this.ID3D11Shader?.Dispose();

        GC.SuppressFinalize(this);
    }
}
