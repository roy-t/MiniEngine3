using Vortice.Direct3D11;

namespace Mini.Engine.DirectX.Buffers;
public sealed class UnorderedAccessView<T> : IDisposable
    where T : unmanaged
{
    internal UnorderedAccessView(ID3D11UnorderedAccessView view)
    {
        this.View = view;
    }

    internal ID3D11UnorderedAccessView View { get; private set; }

    public void Dispose()
    {
        this.View.Dispose();
    }
}
