using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Buffers;

public sealed class RWStructuredBuffer<T> : StructuredBuffer<T>
    where T : unmanaged
{
    private int firstElement;
    private int length;
    private ID3D11UnorderedAccessView? uav;

    public RWStructuredBuffer(Device device, string name, int elements)
        : base(device, name)
    {
        this.EnsureCapacity(elements);
    }

    internal ID3D11UnorderedAccessView GetUnorderedAccessView()
    {
        return this.GetUnorderedAccessView(0, this.Length);
    }

    // TODO: do we really need to cache these?
    internal ID3D11UnorderedAccessView GetUnorderedAccessView(int firstElement, int length)
    {
        if (this.firstElement != firstElement || this.length != length || this.uav == null)
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

            this.uav?.Dispose();
            this.uav = this.Device.CreateUnorderedAccessView(this.Buffer, description);

            this.firstElement = firstElement;
            this.length = length;
        }

        return this.uav;
    }

    protected override ID3D11Buffer CreateBuffer(int sizeInBytes)
    {
        var structuredBufferDesc = new BufferDescription
        {
            Usage = ResourceUsage.Default,
            SizeInBytes = sizeInBytes,
            BindFlags = BindFlags.UnorderedAccess | BindFlags.ShaderResource,
            CpuAccessFlags = CpuAccessFlags.None,
            OptionFlags = ResourceOptionFlags.BufferStructured,
            StructureByteStride = this.PrimitiveSizeInBytes
        };

        return this.Device.CreateBuffer(structuredBufferDesc);
    }

    public override void Dispose()
    {
        this.uav?.Dispose();
        base.Dispose();
    }
}
