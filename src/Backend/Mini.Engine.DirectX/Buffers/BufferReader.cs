using Vortice.Direct3D11;

namespace Mini.Engine.DirectX.Buffers;

public sealed class BufferReader<T> : IDisposable
    where T : unmanaged
{
    private readonly ID3D11DeviceContext Context;
    private readonly ID3D11Buffer Source;
    private readonly MappedSubresource Resource;

    internal BufferReader(ID3D11DeviceContext context, ID3D11Buffer source)
    {
        this.Context = context;
        this.Source = source;

        this.Resource = context.Map(source, 0, MapMode.Read, MapFlags.None);
        context.Flush();
    }    

    public void ReadData(int offset, int length, Span<T> target)
    {
        var span = this.Resource.AsSpan<T>(this.Source);
        var slice = span.Slice(offset, length);
        slice.CopyTo(target);
    }

    public void Dispose()
    {
        this.Context.Unmap(this.Source);
    }
}
