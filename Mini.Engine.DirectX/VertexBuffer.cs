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

        public VertexBuffer(ID3D11Device device, ID3D11DeviceContext context, float growthFactor = 1.1f)
        {
            this.Device = device;
            this.Context = context;
            this.Length = 0;
            this.GrowthFactor = growthFactor;
        }

        public int Length { get; private set; }

        public float GrowthFactor { get; }

        public ID3D11Buffer Buffer { get; private set; }

        public void MapData<T>(T[] vertices)
            where T : struct
            => this.MapData(vertices, Unsafe.SizeOf<T>());

        public void Reserve<T>(int vertexCount)
            => this.Reserve(vertexCount, Unsafe.SizeOf<T>());

        public void Reserve(int vertexCount, int vertexSize)
        {
            if (this.Buffer == null || this.Length < vertexCount)
            {
                this.Buffer?.Release();

                this.Length = (int)(vertexCount * this.GrowthFactor);
                var description = new BufferDescription()
                {
                    Usage = Usage.Dynamic,
                    SizeInBytes = vertexSize * this.Length,
                    BindFlags = BindFlags.VertexBuffer,
                    CpuAccessFlags = CpuAccessFlags.Write,
                };

                this.Buffer = this.Device.CreateBuffer(description);
            }
        }

        public void MapData<T>(T[] vertices, int vertexSize)
            where T : struct
        {
            this.Reserve(vertices.Length, vertexSize);

            var resource = this.Context.Map(this.Buffer, 0, MapMode.WriteDiscard, MapFlags.None);
            Marshal.StructureToPtr(vertices, resource.DataPointer, false);

            this.Context.Unmap(this.Buffer);
        }

        public unsafe void MapData(IntPtr vertices, int vertexCount, int vertexSize, int destinationOffset)
        {
            var totalVertexCount = vertexCount + destinationOffset;
            this.Reserve(totalVertexCount, vertexSize);

            var resource = this.Context.Map(this.Buffer, 0, MapMode.WriteDiscard, MapFlags.None);

            var destinationOffsetInBytes = destinationOffset * vertexSize;
            var destination = resource.DataPointer + destinationOffsetInBytes;
            var destinationSizeInBytes = (this.Length * vertexSize) - destinationOffsetInBytes;
            var copySizeInBytes = vertexSize * vertexCount;

            System.Buffer.MemoryCopy((void*)vertices, (void*)destination, destinationSizeInBytes, copySizeInBytes);

            this.Context.Unmap(this.Buffer);
        }

        public void Dispose() => throw new NotImplementedException();
    }
}
