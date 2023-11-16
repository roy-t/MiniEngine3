using System.Runtime.InteropServices;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX;

namespace Mini.Engine.Graphics.Buffers;

public sealed class UploadBuffer<T>
    where T : unmanaged, IEquatable<T>
{
    private const int MinimumGrowth = 10;
    private const int GrowthFactor = 2;

    public readonly ILifetime<StructuredBuffer<T>> GpuBuffer;
    public readonly ILifetime<ShaderResourceView<T>> GpuBufferView;
    private readonly List<T> CpuBuffer;
    private bool shouldUpload;

    public UploadBuffer(Device device, string name, int initialCapacity)
    {
        var buffer = new StructuredBuffer<T>(device, name, initialCapacity);
        var view = buffer.CreateShaderResourceView();

        this.GpuBuffer = device.Resources.Add(buffer);
        this.GpuBufferView = device.Resources.Add(view);

        this.CpuBuffer = new List<T>(initialCapacity);
        this.shouldUpload = false;
    }

    public int Count => this.CpuBuffer.Count;

    public void Actualize(DeviceContext context)
    {
        if (this.shouldUpload && this.CpuBuffer.Count > 0)
        {
            this.shouldUpload = false;

            var buffer = context.Resources.Get(this.GpuBuffer);
            var capacity = Math.Max(this.CpuBuffer.Count * GrowthFactor, this.CpuBuffer.Count + MinimumGrowth);
            buffer.EnsureCapacity(capacity);
            buffer.MapData(context, CollectionsMarshal.AsSpan(this.CpuBuffer));
        }
    }

    public void Add(in T item)
    {
        this.CpuBuffer.Add(item);
        this.shouldUpload = true;
    }

    public void Remove(in T item)
    {
        this.CpuBuffer.Remove(item);
        this.shouldUpload = true;
    }

    public void Clear()
    {
        this.CpuBuffer.Clear();
        this.shouldUpload = true;
    }
}
