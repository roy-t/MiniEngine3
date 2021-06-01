using System;
using Vortice.Direct3D11;

namespace Mini.Engine.DirectX
{
    public sealed class BufferWriter : IDisposable
    {
        private readonly ID3D11DeviceContext Context;
        private readonly ID3D11Buffer Buffer;
        private readonly int Capacity;
        private readonly int PrimitiveSizeInBytes;
        private readonly MappedSubresource Resource;

        internal BufferWriter(ID3D11DeviceContext context, ID3D11Buffer buffer, int capacity, int primitiveSizeInBytes)
        {
            this.Context = context;
            this.Buffer = buffer;
            this.Capacity = capacity;
            this.PrimitiveSizeInBytes = primitiveSizeInBytes;

            this.Resource = context.Map(buffer, 0, MapMode.WriteDiscard, MapFlags.None);
        }

        public unsafe void MapData(IntPtr vertices, int vertexCount, int offset)
        {
            var destinationOffsetInBytes = offset * this.PrimitiveSizeInBytes;
            var destination = this.Resource.DataPointer + destinationOffsetInBytes;
            var destinationSizeInBytes = (this.Capacity * this.PrimitiveSizeInBytes) - destinationOffsetInBytes;
            var copySizeInBytes = this.PrimitiveSizeInBytes * vertexCount;

            System.Buffer.MemoryCopy((void*)vertices, (void*)destination, destinationSizeInBytes, copySizeInBytes);
        }

        public void Dispose()
            => this.Context.Unmap(this.Buffer);
    }
}
