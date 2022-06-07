using Vortice.Direct3D11;

namespace Mini.Engine.DirectX.Buffers;

public sealed class ConstantBuffer<T> : DeviceBuffer<T>
    where T : unmanaged
{

    // make sure that the CBuffer structure
    // matches the packing rules for CBuffers as described here:
    // https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-packing-rules
    public ConstantBuffer(Device device, string user)
        : base(device, user, "CB")
    {
        this.EnsureCapacity(1);
    }

    protected override ID3D11Buffer CreateBuffer(int sizeInBytes)
    {
        var constBufferDesc = new BufferDescription
        {
            Usage = ResourceUsage.Dynamic,
            ByteWidth = sizeInBytes,
            BindFlags = BindFlags.ConstantBuffer,
            CPUAccessFlags = CpuAccessFlags.Write,
            StructureByteStride = this.PrimitiveSizeInBytes
        };

        return this.Device.CreateBuffer(constBufferDesc);
    }
}
