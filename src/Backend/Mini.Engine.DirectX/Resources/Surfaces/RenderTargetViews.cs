using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Resources.Surfaces;

public static class RenderTargetViews
{
    public static ID3D11RenderTargetView Create(Device device, ID3D11Texture2D texture, string owner, Format format, int arrayIndex, int mipSlice)
    {
        var description = new RenderTargetViewDescription(texture, RenderTargetViewDimension.Texture2DArray, format, mipSlice, arrayIndex, 1);

        var rtv = device.ID3D11Device.CreateRenderTargetView(texture, description);
        rtv.DebugName = DebugNameGenerator.GetName(owner, "RTV", format, (arrayIndex * 1000) + mipSlice);

        if(rtv.DebugName.Contains("2Environment_RTV"))
        {

        }
        return rtv;
    }
}
