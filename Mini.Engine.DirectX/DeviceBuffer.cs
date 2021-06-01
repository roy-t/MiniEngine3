using System;
using Vortice.Direct3D11;

namespace Mini.Engine.DirectX
{
    public abstract class DeviceBuffer : IDisposable
    {
        private static int Counter = 0;

        protected readonly ID3D11Device Device;
        protected readonly ID3D11DeviceContext Context;
        protected readonly int PrimitiveSizeInBytes;
        private readonly int Id;

        internal DeviceBuffer(ID3D11Device device, ID3D11DeviceContext context, int primitiveSizeInBytes)
        {
            this.Device = device;
            this.Context = context;
            this.PrimitiveSizeInBytes = primitiveSizeInBytes;
            this.Id = ++Counter;
        }

        public int Capacity { get; private set; }

        public ID3D11Buffer Buffer { get; private set; }

        public void EnsureCapacity(int primitiveCount, int reserveExtra = 0)
        {
            if (this.Buffer == null || this.Capacity < primitiveCount)
            {
                this.Buffer?.Release();
                this.Capacity = primitiveCount + reserveExtra;
                this.Buffer = this.CreateBuffer(this.Capacity * this.PrimitiveSizeInBytes);
#if DEBUG                                
                this.Buffer.DebugName = $"{this.GetType().Name}[{this.Capacity}]_{this.Id}";
#endif
            }
        }

        public void MapData<T>(params T[] primitives)
            where T : unmanaged
        {
            var resource = this.Context.Map(this.Buffer, 0, MapMode.WriteDiscard, MapFlags.None);

            var span = resource.AsSpan<T>(this.Capacity * this.PrimitiveSizeInBytes);
            primitives.CopyTo(span);

            this.Context.Unmap(this.Buffer);
        }

        public unsafe void MapData(IntPtr primitives, int primitiveCount)
        {
            this.EnsureCapacity(primitiveCount);

            var resource = this.Context.Map(this.Buffer, 0, MapMode.WriteDiscard, MapFlags.None);

            var destination = resource.DataPointer;
            var destinationSizeInBytes = this.Capacity * this.PrimitiveSizeInBytes;
            var copySizeInBytes = this.PrimitiveSizeInBytes * primitiveCount;

            System.Buffer.MemoryCopy((void*)primitives, (void*)destination, destinationSizeInBytes, copySizeInBytes);

            this.Context.Unmap(this.Buffer);
        }

        public BufferWriter OpenWriter()
         => new(this.Context, this.Buffer, this.Capacity, this.PrimitiveSizeInBytes);

        public void Dispose()
            => this.Buffer?.Release();

        protected abstract ID3D11Buffer CreateBuffer(int sizeInBytes);
    }
}
