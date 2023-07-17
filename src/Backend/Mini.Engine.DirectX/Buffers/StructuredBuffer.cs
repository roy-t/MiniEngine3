using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Buffers;

public class StructuredBuffer<T> : DeviceBuffer<T>
    where T : unmanaged
{
    protected StructuredBuffer(Device device, string user, string abbreviation, int capacity = 1)
        : base(device, user, abbreviation)
    {
        this.EnsureCapacity(capacity);        
    }

    public StructuredBuffer(Device device, string user, int capacity = 1)
        : this(device, user, "R", capacity) { }

    public ShaderResourceView<T> CreateShaderResourceView()
    {
        var srv = this.Device.CreateShaderResourceView(this.Buffer, null);
        srv.DebugName = this.Name + $"_SRV_{Guid.NewGuid()}";

        return new ShaderResourceView<T>(srv);
    }

    public ShaderResourceView<T> CreateShaderResourceView(int firstElement, int length)
    {
        var srv = this.CreateSRV(firstElement, length);
        return new ShaderResourceView<T>(srv);
    }

    private ID3D11ShaderResourceView CreateSRV(int firstElement, int length)
    {
        var bufferDescription = new BufferShaderResourceView()
        {
            FirstElement = firstElement,
            NumElements = length
        };

        var description = new ShaderResourceViewDescription()
        {
            Buffer = bufferDescription,
            Format = Format.Unknown,
            ViewDimension = Vortice.Direct3D.ShaderResourceViewDimension.Buffer
        };

        var srv = this.Device.CreateShaderResourceView(this.Buffer, description);        
        srv.DebugName = this.Name + $"_SRV_{Guid.NewGuid()}";

        return srv;
    }

    protected override ID3D11Buffer CreateBuffer(int sizeInBytes)
    {
        var structuredBufferDesc = new BufferDescription
        {
            Usage = ResourceUsage.Dynamic,
            ByteWidth = sizeInBytes,
            BindFlags = BindFlags.ShaderResource,
            CPUAccessFlags = CpuAccessFlags.Write,
            MiscFlags = ResourceOptionFlags.BufferStructured,
            StructureByteStride = PrimitiveSizeInBytes
        };

        return this.Device.CreateBuffer(structuredBufferDesc);
    }

    public override void Dispose()
    {        
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
