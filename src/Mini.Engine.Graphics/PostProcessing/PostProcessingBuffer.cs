using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Surfaces;
using Vortice.DXGI;

namespace Mini.Engine.Graphics.PostProcessing;
public sealed class PostProcessingBuffer : IDisposable
{
    public PostProcessingBuffer(Device device)
    {
        var imageInfo = new ImageInfo(device.Width, device.Height, Format.R16G16B16A16_Float);
        this.Current = new RenderTarget(device, nameof(PostProcessingBuffer) + "A", imageInfo, MipMapInfo.None());
        this.Previous= new RenderTarget(device, nameof(PostProcessingBuffer) + "B", imageInfo, MipMapInfo.None());
    }

    public IRenderTarget Current { get; private set; }
    public IRenderTarget Previous { get; private set; }

    public void Swap()
    {
        var current = this.Current;
        this.Current = this.Previous;
        this.Previous = current;
    }

    public void Dispose()
    {
        this.Current.Dispose();
        this.Previous.Dispose();
    }
}
