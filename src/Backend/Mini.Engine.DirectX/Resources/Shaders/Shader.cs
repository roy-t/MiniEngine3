using Vortice.Direct3D;
using Vortice.Direct3D11;

namespace Mini.Engine.DirectX.Resources.Shaders;

public interface IShader : IDisposable
{
    public string Name { get; }
}

// TODO: we can probably get rid of this soon!
public abstract class Shader<TShader> : IDisposable
    where TShader : ID3D11DeviceChild
{
    protected readonly Device Device;
    protected Blob blob;

    public Shader(Device device, string name)
    {
        this.Device = device;
        this.blob = null!;
        this.ID3D11Shader = null!;
        this.Name = name;
    }

    public string Name { get; }

    public TShader ID3D11Shader { get; set; } 

    public virtual void Dispose()
    {
        this.blob?.Dispose();
        this.ID3D11Shader?.Dispose();

        GC.SuppressFinalize(this);
    }
}
