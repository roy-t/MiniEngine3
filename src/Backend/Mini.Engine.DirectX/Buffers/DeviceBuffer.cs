using System;
using Mini.Engine.DirectX.Contexts;
using Vortice.Direct3D11;

namespace Mini.Engine.DirectX.Buffers;

public abstract class DeviceBuffer<T> : IDisposable
    where T : unmanaged
{
    private static int Counter = 0;
    protected readonly ID3D11Device Device;

    internal DeviceBuffer(Device device, string name)
    {
        this.Device = device.ID3D11Device;
        unsafe
        {
            this.PrimitiveSizeInBytes = sizeof(T);
        }

        this.Name = $"{name}#{++Counter}";
    }

    internal int PrimitiveSizeInBytes { get; }

    public int Capacity { get; private set; }

    public int Length { get; private set; }

    internal ID3D11Buffer Buffer { get; private set; } = null!;

    public string Name { get; }

    public void EnsureCapacity(int primitiveCount, int reserveExtra = 0)
    {
        if (this.Buffer == null || this.Capacity < primitiveCount)
        {
            this.Buffer?.Dispose();
            this.Capacity = primitiveCount + reserveExtra;
            this.Length = primitiveCount;
            this.Buffer = this.CreateBuffer(this.Capacity * this.PrimitiveSizeInBytes);
#if DEBUG
            this.Buffer.DebugName = this.Name;
#endif
        }
    }

    public void MapData(DeviceContext context, params T[] primitives)
    {
        this.EnsureCapacity(primitives.Length);

        var ctx = context.ID3D11DeviceContext;
        var resource = ctx.Map(this.Buffer, 0, MapMode.WriteDiscard, MapFlags.None);
        var span = resource.AsSpan<T>(this.Buffer);

        primitives.CopyTo(span);

        ctx.Unmap(this.Buffer);
    }

    public void MapData(DeviceContext context, Span<T> primitives)
    {
        this.EnsureCapacity(primitives.Length);

        var ctx = context.ID3D11DeviceContext;
        var resource = ctx.Map(this.Buffer, 0, MapMode.WriteDiscard, MapFlags.None);
        var span = resource.AsSpan<T>(this.Buffer);

        primitives.CopyTo(span);

        ctx.Unmap(this.Buffer);
    }

    public BufferWriter<T> OpenWriter(DeviceContext context)
    {
        return new(context.ID3D11DeviceContext, this.Buffer);
    }

    public BufferReader<T> OpenReader(DeviceContext context, StagingBuffer<T> staging)
    {
        return new(context.ID3D11DeviceContext, this.Buffer, staging.Buffer);
    }

    public virtual void Dispose()
    {
        this.Buffer?.Dispose();
        GC.SuppressFinalize(this);
    }

    protected abstract ID3D11Buffer CreateBuffer(int sizeInBytes);
}
