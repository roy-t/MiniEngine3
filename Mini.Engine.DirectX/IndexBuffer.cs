using Vortice.Direct3D11;

namespace Mini.Engine.DirectX
{
    public enum IndexSize
    {
        TwoByte = 2,
        FourByte = 4
    }

    public sealed class IndexBuffer : DeviceBuffer
    {
        public IndexBuffer(ID3D11Device device, ID3D11DeviceContext context, IndexSize indexSize)
            : base(device, context, (int)indexSize) { }

        protected override ID3D11Buffer CreateBuffer(int sizeInBytes)
        {
            var description = new BufferDescription()
            {
                Usage = Usage.Dynamic,
                SizeInBytes = sizeInBytes,
                BindFlags = BindFlags.IndexBuffer,
                CpuAccessFlags = CpuAccessFlags.Write,
            };

            return this.Device.CreateBuffer(description);
        }
    }
}
