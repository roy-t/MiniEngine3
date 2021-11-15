using Vortice.Direct3D11;

namespace Mini.Engine.DirectX;

public sealed class VertexBuffer<T> : DeviceBuffer<T>
    where T : unmanaged
{
    public VertexBuffer(Device device)
        : base(device) { }

    protected override ID3D11Buffer CreateBuffer(int sizeInBytes)
    {
        var description = new BufferDescription()
        {
            Usage = ResourceUsage.Dynamic,
            SizeInBytes = sizeInBytes,
            BindFlags = BindFlags.VertexBuffer,
            CpuAccessFlags = CpuAccessFlags.Write,
        };

        return this.Device.CreateBuffer(description);
    }
}
