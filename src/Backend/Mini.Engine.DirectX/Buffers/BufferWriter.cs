using Vortice.Direct3D11;

namespace Mini.Engine.DirectX.Buffers;

public sealed class BufferWriter<T> : IDisposable
    where T : unmanaged
{
    private readonly ID3D11DeviceContext Context;
    private readonly ID3D11Buffer Buffer;
    private readonly MappedSubresource Resource;

    internal BufferWriter(ID3D11DeviceContext context, ID3D11Buffer buffer)
    {
        this.Context = context;
        this.Buffer = buffer;
        this.Resource = context.Map(buffer, 0, MapMode.WriteDiscard, MapFlags.None);
    }

    public void MapData(Span<T> vertices, int offset)
    {
        var span = this.Resource.AsSpan<T>(this.Buffer);
        var slice = span.Slice(offset, vertices.Length);
        vertices.CopyTo(slice);
    }

    public void Dispose()
    {
        this.Context.Unmap(this.Buffer);
    }
}
