using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Resources;

public static class RenderTargetViews
{
    public static ID3D11RenderTargetView Create(Device device, ID3D11Texture2D texture, Format format, string textureName)
    {
        var description = new RenderTargetViewDescription(texture, RenderTargetViewDimension.Texture2D, format);
        var rtv = device.ID3D11Device.CreateRenderTargetView(texture, description);
        rtv.DebugName = $"{textureName}_RTV";

        return rtv;
    }

    public static ID3D11RenderTargetView Create(Device device, ID3D11Texture2D texture, Format format, int arrayIndex, string textureName)
    {
        var description = new RenderTargetViewDescription(texture, RenderTargetViewDimension.Texture2DArray, format, 0, arrayIndex, 1);
        
        var rtv = device.ID3D11Device.CreateRenderTargetView(texture, description);
        rtv.DebugName = $"{textureName}[{arrayIndex}]_RTV";

        return rtv;
    }
}
