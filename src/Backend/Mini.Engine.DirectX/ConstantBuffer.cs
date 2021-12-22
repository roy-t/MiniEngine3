using Vortice.Direct3D11;

namespace Mini.Engine.DirectX;

public sealed class ConstantBuffer<T> : DeviceBuffer<T>
    where T : unmanaged
{
    public ConstantBuffer(Device device, string name)
        : base(device, name)
    {
        this.EnsureCapacity(1);
    }

    protected override ID3D11Buffer CreateBuffer(int sizeInBytes)
    {
        var constBufferDesc = new BufferDescription
        {
            Usage = ResourceUsage.Dynamic,
            SizeInBytes = sizeInBytes,
            BindFlags = BindFlags.ConstantBuffer,
            CpuAccessFlags = CpuAccessFlags.Write
        };

        return this.Device.CreateBuffer(constBufferDesc);
    }
}
