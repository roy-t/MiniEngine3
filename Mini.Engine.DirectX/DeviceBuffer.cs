using System;
using Vortice.Direct3D11;

namespace Mini.Engine.DirectX
{
    public abstract class DeviceBuffer<T> : IDisposable
        where T : unmanaged
    {
        private static int Counter = 0;

        protected readonly ID3D11Device Device;
        protected readonly ID3D11DeviceContext Context;
        protected readonly int PrimitiveSizeInBytes;
        private readonly int Id;

        internal DeviceBuffer(ID3D11Device device, ID3D11DeviceContext context)
        {
            this.Device = device;
            this.Context = context;
            unsafe
            {
                this.PrimitiveSizeInBytes = sizeof(T);
            }

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

        public void MapData(params T[] primitives)
        {
            this.EnsureCapacity(primitives.Length);

            var resource = this.Context.Map(this.Buffer, 0, MapMode.WriteDiscard, MapFlags.None);
            var span = resource.AsSpan<T>(this.Buffer);

            primitives.CopyTo(span);

            this.Context.Unmap(this.Buffer);
        }

        public void MapData(Span<T> primitives)
        {
            this.EnsureCapacity(primitives.Length);

            var resource = this.Context.Map(this.Buffer, 0, MapMode.WriteDiscard, MapFlags.None);
            var span = resource.AsSpan<T>(this.Buffer);

            primitives.CopyTo(span);

            this.Context.Unmap(this.Buffer);
        }

        public BufferWriter<T> OpenWriter()
         => new(this.Context, this.Buffer);

        public void Dispose()
            => this.Buffer?.Release();

        protected abstract ID3D11Buffer CreateBuffer(int sizeInBytes);
    }
}
