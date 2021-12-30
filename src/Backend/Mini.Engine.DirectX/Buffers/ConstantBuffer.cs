using Vortice.Direct3D11;

namespace Mini.Engine.DirectX.Buffers;

public sealed class ConstantBuffer<T> : DeviceBuffer<T>
    where T : unmanaged
{
    public ConstantBuffer(Device device, string name)
        : base(device, name)
    {
        this.EnsureCapacity(1);
    }

    // When you get an SEHException, make sure that the CBuffer structure
    // matches the packing rules for CBuffers as described here:
    // https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-packing-rules
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
