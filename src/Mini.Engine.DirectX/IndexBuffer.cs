using Vortice.Direct3D11;

namespace Mini.Engine.DirectX
{
    public sealed class IndexBuffer<T> : DeviceBuffer<T>
        where T : unmanaged
    {
        public IndexBuffer(ID3D11Device device)
            : base(device) { }

        protected override ID3D11Buffer CreateBuffer(int sizeInBytes)
        {
            var description = new BufferDescription()
            {
                Usage = ResourceUsage.Dynamic,
                SizeInBytes = sizeInBytes,
                BindFlags = BindFlags.IndexBuffer,
                CpuAccessFlags = CpuAccessFlags.Write,
            };

            return this.Device.CreateBuffer(description);
        }
    }
}
