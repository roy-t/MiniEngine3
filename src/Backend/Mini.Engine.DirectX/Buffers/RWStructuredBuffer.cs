using Mini.Engine.DirectX.Contexts;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Buffers;

public sealed class RWStructuredBuffer<T> : StructuredBuffer<T>
    where T : unmanaged
{
    public RWStructuredBuffer(Device device, string user, int elements)
        : base(device, user, "RW")
    {
        this.EnsureCapacity(elements);
    }

    public BufferReader<T> OpenReader(DeviceContext context)
    {
        return new(context.ID3D11DeviceContext, this.Buffer);
    }

    public void ReadData(DeviceContext context, Span<T> output)
    {
        var ctx = context.ID3D11DeviceContext;
        var resource = ctx.Map(this.Buffer, 0, MapMode.Read, Vortice.Direct3D11.MapFlags.None);
        ctx.Flush();

        var span = resource.AsSpan<T>(this.Buffer);
        span.CopyTo(output);

        ctx.Unmap(this.Buffer);
    }

    public void CopyToBuffer(DeviceContext context, DeviceBuffer<T> deviceBuffer)
    {
        var ctx = context.ID3D11DeviceContext;

        var source = ctx.Map(this.Buffer, 0, MapMode.Read, Vortice.Direct3D11.MapFlags.None);
        var target = ctx.Map(deviceBuffer.Buffer, 0, MapMode.WriteDiscard, Vortice.Direct3D11.MapFlags.None);

        ctx.Flush();

        var sourceSpan = source.AsSpan<T>(this.Buffer);
        var targetSpan = target.AsSpan<T>(deviceBuffer.Buffer);

        sourceSpan.CopyTo(targetSpan);

        ctx.Unmap(this.Buffer);
        ctx.Unmap(deviceBuffer.Buffer);
    }

    public UnorderedAccessView<T> CreateUnorderedAccessView()
    {
        var uav = this.Device.CreateUnorderedAccessView(this.Buffer, null);
        uav.DebugName = this.Name + $"_UAV_{Guid.NewGuid()}";

        return new UnorderedAccessView<T>(uav);
    }

    public UnorderedAccessView<T> CreateUnorderedAccessView(int firstElement, int length)
    {
        var uav = this.CreateUAV(firstElement, length);
        return new UnorderedAccessView<T>(uav);
    }

    private ID3D11UnorderedAccessView CreateUAV(int firstElement, int length)
    {
        var bufferDescription = new BufferUnorderedAccessView()
        {
            FirstElement = firstElement,
            NumElements = length,
            Flags = BufferUnorderedAccessViewFlags.None
        };

        var description = new UnorderedAccessViewDescription()
        {
            Buffer = bufferDescription,
            Format = Format.Unknown,
            ViewDimension = UnorderedAccessViewDimension.Buffer
        };


        var uav = this.Device.CreateUnorderedAccessView(this.Buffer, description);
        uav.DebugName = this.Name + $"_UAV_{Guid.NewGuid()}";

        return uav;
    }

    protected override ID3D11Buffer CreateBuffer(int sizeInBytes)
    {
        var structuredBufferDesc = new BufferDescription
        {
            Usage = ResourceUsage.Default,
            ByteWidth = sizeInBytes,
            BindFlags = BindFlags.UnorderedAccess | BindFlags.ShaderResource,
            CPUAccessFlags = CpuAccessFlags.Read | CpuAccessFlags.Write,
            MiscFlags = ResourceOptionFlags.BufferStructured,
            StructureByteStride = this.PrimitiveSizeInBytes
        };

        return this.Device.CreateBuffer(structuredBufferDesc);
    }

    public override void Dispose()
    {
        base.Dispose();
    }
}
