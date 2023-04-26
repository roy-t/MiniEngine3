using System.Diagnostics.CodeAnalysis;
using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.Graphics.Diesel;
using Mini.Engine.Graphics.PostProcessing;
using Mini.Engine.UI;
using Vortice.DXGI;

namespace Mini.Engine.Diesel;

[Service]
internal sealed class DieselGameLoop : IGameLoop
{
    private readonly Device Device;
    private readonly ContentManager Content;
    private readonly DieselUserInterface UserInterface;
    private readonly DieselUpdateLoop UpdateLoop;
    private readonly DieselRenderLoop RenderLoop;

    private RenderTarget albedo;
    private DepthStencilBuffer depthStencilBuffer;

    private readonly PresentationHelper PresentationHelper;
    private readonly CameraService CameraService;

    public DieselGameLoop(Device device, ContentManager content, DieselUserInterface userInterface, DieselUpdateLoop updateLoop, DieselRenderLoop renderLoop, PresentationHelper presentationHelper, CameraService cameraService)
    {
        this.Device = device;
        this.Content = content;
        this.UserInterface = userInterface;
        this.UpdateLoop = updateLoop;
        this.RenderLoop = renderLoop;
        this.PresentationHelper = presentationHelper;
        this.CameraService = cameraService;

        this.Device.Resources.PushFrame("Diesel");

        this.CameraService.InitializePrimaryCamera(device.Width, device.Height);

        this.Resize(device.Width, device.Height);
    }

    public void Update(float elapsedSimulationTime, float elapsedRealWorldTime)
    {
        this.Content.ReloadChangedContent();

        this.UpdateLoop.Run(elapsedSimulationTime);
        this.UserInterface.NewFrame(elapsedRealWorldTime);
    }

    public void Draw(float alpha, float elapsedRealWorldTime)
    {
        this.RenderLoop.Run(this.albedo, this.depthStencilBuffer, 0, 0, this.Device.Width, this.Device.Height, alpha, elapsedRealWorldTime);
        this.PresentationHelper.ToneMapAndPresent(this.Device.ImmediateContext, this.albedo);

        this.UserInterface.Render();
    }

    [MemberNotNull(nameof(albedo), nameof(depthStencilBuffer))]
    public void Resize(int width, int height)
    {
        this.albedo?.Dispose();
        this.depthStencilBuffer?.Dispose();

        var imageInfo = new ImageInfo(width, height, Format.R8G8B8A8_UNorm);
        this.albedo = new RenderTarget(this.Device, nameof(this.albedo), imageInfo, MipMapInfo.None());
        this.depthStencilBuffer = new DepthStencilBuffer(this.Device, nameof(this.depthStencilBuffer), DepthStencilFormat.D32_Float, width, height, 1);

        this.CameraService.Resize(width, height);
        this.UserInterface.Resize(width, height);
    }

    public void Dispose()
    {
        this.albedo.Dispose();
        this.depthStencilBuffer.Dispose();

        this.Device.Resources.PopFrame();
    }
}
