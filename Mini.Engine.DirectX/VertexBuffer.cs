using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Vortice.Direct3D11;

namespace Mini.Engine.DirectX
{
    public sealed class VertexBuffer : IDisposable
    {
        private readonly ID3D11Device Device;
        private readonly ID3D11DeviceContext Context;
        private ID3D11Buffer buffer;

        public VertexBuffer(ID3D11Device device, ID3D11DeviceContext context, int size)
        {
            this.Device = device;
            this.Context = context;
            this.Length = size;
        }

        public int Length { get; private set; }

        public void MapData<T>(T[] vertices, int reserve = 0)
            where T : struct
            => this.MapData(vertices, Unsafe.SizeOf<T>(), reserve);

        public void MapData<T>(T[] vertices, int vertexSize, int reserve = 0)
            where T : struct
        {
            this.EnsureBufferIsCreatedAndIsLargeEnough(vertices.Length, vertexSize, reserve);

            var resource = this.Context.Map(this.buffer, 0, MapMode.WriteDiscard, MapFlags.None);
            Marshal.StructureToPtr(vertices, resource.DataPointer, false);
        }

        public unsafe void MapData(IntPtr vertices, int vertexCount, int vertexSize, int destinationOffsetInBytes, int reserve = 0)
        {
            this.EnsureBufferIsCreatedAndIsLargeEnough(vertexCount, vertexSize, reserve);

            var resource = this.Context.Map(this.buffer, 0, MapMode.WriteDiscard, MapFlags.None);

            var destination = resource.DataPointer + destinationOffsetInBytes;
            var destinationSize = (this.Length * vertexSize) - destinationOffsetInBytes;
            var copySize = vertexSize * vertexCount;

            Buffer.MemoryCopy((void*)vertices, (void*)destination, destinationSize, copySize);
        }

        private void EnsureBufferIsCreatedAndIsLargeEnough(int vertexCount, int vertexSize, int reserve = 0)
        {
            if (this.buffer == null || this.Length < vertexCount)
            {
                this.buffer?.Release();

                this.Length = vertexCount + reserve;
                var description = new BufferDescription()
                {
                    Usage = Usage.Dynamic,
                    SizeInBytes = vertexSize * this.Length,
                    BindFlags = BindFlags.VertexBuffer,
                    CpuAccessFlags = CpuAccessFlags.Write,
                    OptionFlags = ResourceOptionFlags.None,
                    StructureByteStride = 0
                };

                this.buffer = this.Device.CreateBuffer(description);
            }
        }

        public void Dispose() => throw new NotImplementedException();
    }
}
