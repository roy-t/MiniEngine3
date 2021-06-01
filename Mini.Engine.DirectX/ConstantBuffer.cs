using Vortice.Direct3D11;

namespace Mini.Engine.DirectX
{
    public sealed class ConstantBuffer : DeviceBuffer
    {
        public ConstantBuffer(ID3D11Device device, ID3D11DeviceContext context, int sizeInBytes)
            : base(device, context, sizeInBytes)
        {
            this.EnsureCapacity(1);
        }

        protected override ID3D11Buffer CreateBuffer(int sizeInBytes)
        {
            var constBufferDesc = new BufferDescription
            {
                Usage = Usage.Dynamic,
                SizeInBytes = sizeInBytes,
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.Write
            };

            return this.Device.CreateBuffer(constBufferDesc);
        }
    }
}
