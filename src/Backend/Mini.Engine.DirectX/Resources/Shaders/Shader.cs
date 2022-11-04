using Mini.Engine.DirectX.Buffers;
using Vortice.Direct3D;
using Vortice.Direct3D11;

namespace Mini.Engine.DirectX.Resources.Shaders;

public interface IShader : IDisposable
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

    public TShader ID3D11Shader { get; set; } // TODO: we can probably get rid of the set method soon!

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
