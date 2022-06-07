using Vortice.DXGI;

namespace Mini.Engine.DirectX;
public static class FormatExtensions
{
    public static int BytesPerPixel(this Format format)
    {
        return format.BitsPerPixel() / 8;
    }
}
