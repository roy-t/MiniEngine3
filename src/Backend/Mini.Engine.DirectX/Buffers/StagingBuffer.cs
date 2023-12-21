using Mini.Engine.DirectX.Resources.Surfaces;
using Vortice.Direct3D11;

namespace Mini.Engine.DirectX.Buffers;

// TODO: see DeviceContext.CopySurfaceToCPU and this class and make it into a BufferReader/Writer equivalent for textures!
public sealed class StagingBuffer<T> : IDisposable
    where T : unmanaged
{
    private static int Counter = 0;

    public StagingBuffer(Device device, ImageInfo imageInfo, MipMapInfo mipMapInfo, string name)
    {
        this.Name = $"{name}#{++Counter}";

        unsafe
        {
            this.PrimitiveSizeInBytes = sizeof(T);
        }

        var description = Textures.CreateDescription(imageInfo, mipMapInfo, BindInfo.Staging, ResourceInfo.Texture, SamplingInfo.None);
       
        this.Buffer = device.ID3D11Device.CreateTexture2D(description);
#if DEBUG
        this.Buffer.DebugName = this.Name;
#endif
    }

    public string Name { get; }
    internal ID3D11Texture2D Buffer { get; }
    internal int PrimitiveSizeInBytes { get; }

    public void Dispose()
    {
        this.Buffer.Dispose();
    }
}

