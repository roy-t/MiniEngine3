using System;
using Vortice.Direct3D11;

namespace Mini.Engine.DirectX;

public abstract class DeviceBuffer<T> : IDisposable
    where T : unmanaged
{
    private static int Counter = 0;

    protected readonly ID3D11Device Device;
    private readonly int Id;

    internal DeviceBuffer(Device device)
    {
        this.Device = device.ID3D11Device;
        unsafe
        {
            this.PrimitiveSizeInBytes = sizeof(T);
        }

        this.Id = ++Counter;
    }

    internal int PrimitiveSizeInBytes { get; }

    public int Capacity { get; private set; }

    public ID3D11Buffer Buffer { get; private set; } = null!;

    public void EnsureCapacity(int primitiveCount, int reserveExtra = 0)
    {
        if (this.Buffer == null || this.Capacity < primitiveCount)
        {
            this.Buffer?.Dispose();
            this.Capacity = primitiveCount + reserveExtra;
            this.Buffer = this.CreateBuffer(this.Capacity * this.PrimitiveSizeInBytes);
#if DEBUG
            this.Buffer.DebugName = $"{this.GetType().Name}[{this.Capacity}]_{this.Id}";
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

    public void Dispose()
    {
        this.Buffer?.Dispose();
    }

    protected abstract ID3D11Buffer CreateBuffer(int sizeInBytes);
}
