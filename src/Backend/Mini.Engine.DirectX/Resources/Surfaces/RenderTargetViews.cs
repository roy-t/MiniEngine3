using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX.Resources.Surfaces;

public static class RenderTargetViews
{
    public static ID3D11RenderTargetView Create(Device device, ID3D11Texture2D texture, Format format, int arrayIndex, int mipSlice, string user, string meaning)
    {
        var description = new RenderTargetViewDescription(texture, RenderTargetViewDimension.Texture2DArray, format, mipSlice, arrayIndex, 1);

        var rtv = device.ID3D11Device.CreateRenderTargetView(texture, description);
        rtv.DebugName = DebugNameGenerator.GetName(user, "RTV", meaning, format, (arrayIndex * 1000) + mipSlice);

        return rtv;
    }
}
