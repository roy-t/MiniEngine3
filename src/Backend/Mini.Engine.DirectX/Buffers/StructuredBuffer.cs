using System;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Buffers;

public class StructuredBuffer<T> : DeviceBuffer<T>
    where T : unmanaged
{
    private int firstElement;
    private int length;
    private ID3D11ShaderResourceView? srv;


    public StructuredBuffer(Device device, string name)
        : base(device, name)
    {
        this.EnsureCapacity(1);
    }

    internal ID3D11ShaderResourceView GetShaderResourceView()
    {
        return this.GetShaderResourceView(0, this.Length);
    }

    // TODO: do we really need to cache these?
    internal ID3D11ShaderResourceView GetShaderResourceView(int firstElement, int length)
    {
        if (this.firstElement != firstElement || this.length != length || this.srv == null)
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

            this.srv?.Dispose();
            this.srv = this.Device.CreateShaderResourceView(this.Buffer, description);

            this.firstElement = firstElement;
            this.length = length;
        }

        return this.srv;
    }

    protected override ID3D11Buffer CreateBuffer(int sizeInBytes)
    {
        var structuredBufferDesc = new BufferDescription
        {
            Usage = ResourceUsage.Dynamic,
            SizeInBytes = sizeInBytes,
            BindFlags = BindFlags.ShaderResource,
            CpuAccessFlags = CpuAccessFlags.Write,
            OptionFlags = ResourceOptionFlags.BufferStructured,
            StructureByteStride = PrimitiveSizeInBytes
        };

        return this.Device.CreateBuffer(structuredBufferDesc);
    }

    public override void Dispose()
    {
        this.srv?.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
