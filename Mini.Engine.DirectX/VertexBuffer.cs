using System;
using System.Runtime.InteropServices;
using Vortice.Direct3D11;

namespace Mini.Engine.DirectX
{
    public sealed class VertexBuffer : IDisposable
    {
        private readonly ID3D11Device Device;
        private readonly ID3D11DeviceContext Context;
        private readonly int VertexSizeInBytes;

        public VertexBuffer(ID3D11Device device, ID3D11DeviceContext context, int vertexSizeInBytes, float growthFactor = 1.1f)
        {
            this.Device = device;
            this.Context = context;
            this.VertexSizeInBytes = vertexSizeInBytes;
            this.GrowthFactor = growthFactor;
            this.Capacity = 0;
        }

        public int Capacity { get; private set; }

        public float GrowthFactor { get; }

        public ID3D11Buffer Buffer { get; private set; }

        public void Reserve(int vertexCount)
        {
            if (this.Buffer == null || this.Capacity < vertexCount)
            {
                this.Buffer?.Release();

                this.Capacity = (int)(vertexCount * this.GrowthFactor);
                var description = new BufferDescription()
                {
                    Usage = Usage.Dynamic,
                    SizeInBytes = this.VertexSizeInBytes * this.Capacity,
                    BindFlags = BindFlags.VertexBuffer,
                    CpuAccessFlags = CpuAccessFlags.Write,
                };

                this.Buffer = this.Device.CreateBuffer(description);
            }
        }

        public void MapData<T>(T[] vertices)
            where T : struct
        {
            this.Reserve(vertices.Length);

            var resource = this.Context.Map(this.Buffer, 0, MapMode.WriteDiscard, MapFlags.None);
            Marshal.StructureToPtr(vertices, resource.DataPointer, false);

            this.Context.Unmap(this.Buffer);
        }

        public unsafe void MapData(IntPtr vertices, int vertexCount)
        {
            this.Reserve(vertexCount);

            var resource = this.Context.Map(this.Buffer, 0, MapMode.WriteDiscard, MapFlags.None);

            var destination = resource.DataPointer;
            var destinationSizeInBytes = this.Capacity * this.VertexSizeInBytes;
            var copySizeInBytes = this.VertexSizeInBytes * vertexCount;

            System.Buffer.MemoryCopy((void*)vertices, (void*)destination, destinationSizeInBytes, copySizeInBytes);

            this.Context.Unmap(this.Buffer);
        }

        public unsafe void MapData(IntPtr[] data, int[] vertexCounts)
        {
            var vertexTotal = 0;
            for (var i = 0; i < vertexCounts.Length; i++)
            {
                vertexTotal += vertexCounts[i];
            }

            this.Reserve(vertexTotal);
            var resource = this.Context.Map(this.Buffer, 0, MapMode.WriteDiscard, MapFlags.None);

            var offset = 0;
            for (var i = 0; i < data.Length; i++)
            {
                var destinationOffsetInBytes = offset * this.VertexSizeInBytes;
                var destination = resource.DataPointer + destinationOffsetInBytes;
                var destinationSizeInBytes = (this.Capacity * this.VertexSizeInBytes) - destinationOffsetInBytes;
                var copySizeInBytes = this.VertexSizeInBytes * vertexCounts[i];

                System.Buffer.MemoryCopy((void*)data[i], (void*)destination, destinationSizeInBytes, copySizeInBytes);

                offset += vertexCounts[i];
            }

            this.Context.Unmap(this.Buffer);
        }

        public Writer OpenWriter()
         => new(this.Context, this.Buffer, this.Capacity, this.VertexSizeInBytes);

        public void Dispose() => throw new NotImplementedException();

        public sealed class Writer : IDisposable
        {
            private readonly ID3D11DeviceContext Context;
            private readonly ID3D11Buffer Buffer;
            private readonly int Capacity;
            private readonly int VertexSizeInBytes;
            private readonly MappedSubresource Resource;

            internal Writer(ID3D11DeviceContext context, ID3D11Buffer buffer, int capacity, int vertexSizeInBytes)
            {
                this.Context = context;
                this.Buffer = buffer;
                this.Capacity = capacity;
                this.VertexSizeInBytes = vertexSizeInBytes;

                this.Resource = context.Map(buffer, 0, MapMode.WriteDiscard, MapFlags.None);
            }

            public unsafe void MapData(IntPtr vertices, int vertexCount, int offset)
            {
                var destinationOffsetInBytes = offset * this.VertexSizeInBytes;
                var destination = this.Resource.DataPointer + destinationOffsetInBytes;
                var destinationSizeInBytes = (this.Capacity * this.VertexSizeInBytes) - destinationOffsetInBytes;
                var copySizeInBytes = this.VertexSizeInBytes * vertexCount;

                System.Buffer.MemoryCopy((void*)vertices, (void*)destination, destinationSizeInBytes, copySizeInBytes);
            }

            public void Dispose()
                => this.Context.Unmap(this.Buffer);
        }
    }
}
