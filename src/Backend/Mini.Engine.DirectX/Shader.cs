using System;
using Vortice.Direct3D;
using Vortice.Direct3D11;

namespace Mini.Engine.DirectX;

public interface IPixelShader
{
    internal ID3D11PixelShader ID3D11Shader { get; set; }
}

public interface IVertexShader
{
    internal ID3D11VertexShader ID3D11Shader { get; set; }
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
