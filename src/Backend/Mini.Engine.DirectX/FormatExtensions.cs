using Vortice.DXGI;

namespace Mini.Engine.DirectX;
public static class FormatExtensions
{
    // TODO: tnis extension is no 
    public static int BytesPerPixel(this Format format)
    {
        
        return format.GetBitsPerPixel() / 8;
    }
}
