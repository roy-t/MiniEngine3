using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Buffers;

public sealed class IndexBuffer<T> : DeviceBuffer<T>
    where T : unmanaged
{
    public IndexBuffer(Device device, string user)
        : base(device, user, "IB")
    {
        if (this.PrimitiveSizeInBytes == 2)
        {
            this.Format = Format.R16_UInt;
        }
        else if (this.PrimitiveSizeInBytes == 4)
        {
            this.Format = Format.R32_UInt;
        }
        else
        {
            throw new ArgumentException("Argument <T> should be a type of 2 or 4 bytes to be a valid index type");
        }
    }

    internal Format Format { get; }

    protected override ID3D11Buffer CreateBuffer(int sizeInBytes)
    {
        var description = new BufferDescription()
        {
            Usage = ResourceUsage.Dynamic,
            ByteWidth = sizeInBytes,
            BindFlags = BindFlags.IndexBuffer,
            CPUAccessFlags = CpuAccessFlags.Write,
            StructureByteStride = this.PrimitiveSizeInBytes
        };

        return this.Device.CreateBuffer(description);
    }
}
