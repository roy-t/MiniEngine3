using Vortice.Direct3D11;

namespace Mini.Engine.DirectX.Buffers;
public sealed class ShaderResourceView<T> : IDisposable
    where T : unmanaged
{
    internal ShaderResourceView(ID3D11ShaderResourceView view)
    {
        this.View = view;
    }

    internal ID3D11ShaderResourceView View { get; private set; }

    public void Dispose()
    {
        this.View.Dispose();
    }
}
