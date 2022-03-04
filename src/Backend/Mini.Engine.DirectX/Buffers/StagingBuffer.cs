using System;
using Vortice.Direct3D11;

namespace Mini.Engine.DirectX.Buffers;
public sealed class StagingBuffer<T> : IDisposable
    where T : unmanaged
{
    private static int Counter = 0;
    
    public StagingBuffer(Device device, int elements, string name)
    {
        this.Name = $"{name}#{++Counter}";

        unsafe
        {
            this.PrimitiveSizeInBytes = sizeof(T);
        }

        var description = new BufferDescription()
        {
            BindFlags = BindFlags.None,
            Usage = ResourceUsage.Staging,
            CpuAccessFlags = CpuAccessFlags.Read | CpuAccessFlags.Write,
            OptionFlags = ResourceOptionFlags.None,
            StructureByteStride = this.PrimitiveSizeInBytes,
            SizeInBytes = this.PrimitiveSizeInBytes * elements
        };

        this.Buffer = device.ID3D11Device.CreateBuffer(description);
#if DEBUG
        this.Buffer.DebugName = this.Name;
#endif
    }

    public string Name { get; }
    internal ID3D11Buffer Buffer { get; }
    internal int PrimitiveSizeInBytes { get; }

    public void Dispose()
    {
        this.Buffer.Dispose();
    }
}
