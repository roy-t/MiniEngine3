using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.Graphics.Diesel;
using Mini.Engine.Graphics.PostProcessing;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace Mini.Engine.Diesel;

[Service]
internal sealed class DieselGameLoop : IGameLoop
{
    private readonly Device Device;

    private RenderTarget albedo;
    private DepthStencilBuffer depthStencilBuffer;

    private readonly PresentationHelper PresentationHelper;

    public DieselGameLoop(Device device, PresentationHelper presentationHelper)
    {
        this.Device = device;
        this.PresentationHelper = presentationHelper;

        this.Resize(device.Width, device.Height);
    }

    public void Update(float time, float elapsed)
    {
    }

    public void Draw(float alpha)
    {
        ClearBuffersSystem.Clear(this.Device, this.albedo, Colors.Transparent);
        ClearBuffersSystem.Clear(this.Device, this.depthStencilBuffer);

        this.PresentationHelper.ToneMapAndPresent(this.Device.ImmediateContext, this.albedo);
    }

    [MemberNotNull(nameof(albedo), nameof(depthStencilBuffer))]
    public void Resize(int width, int height)
    {
        this.albedo?.Dispose();
        this.depthStencilBuffer?.Dispose();

        var imageInfo = new ImageInfo(width, height, Format.R8G8B8A8_UNorm);
        this.albedo = new RenderTarget(this.Device, nameof(this.albedo), imageInfo, MipMapInfo.None());
        this.depthStencilBuffer = new DepthStencilBuffer(this.Device, nameof(this.depthStencilBuffer), DepthStencilFormat.D32_Float, width, height, 1);
    }

    public void Dispose()
    {
        this.albedo.Dispose();
        this.depthStencilBuffer.Dispose();        
    }
}
