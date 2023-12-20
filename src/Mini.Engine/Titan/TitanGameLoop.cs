using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.Graphics.PostProcessing;
using Mini.Engine.Titan.Graphics;
using Mini.Engine.UI;

namespace Mini.Engine.Titan;

[Service]
internal class TitanGameLoop : IGameLoop
{
    private readonly Device Device;
    private readonly UICore UserInterface;
    private readonly PresentationHelper Presenter;
    private readonly GBuffer GBuffer;
    private readonly CameraController CameraController;
    private readonly TerrainRenderer TerrainRenderer;

    public TitanGameLoop(Device device, UICore userInterface, PresentationHelper presenter, CameraController cameraController, TerrainRenderer terrainRenderer)
    {
        this.GBuffer = new GBuffer(device);                

        this.Device = device;
        this.UserInterface = userInterface;
        this.CameraController = cameraController;
        this.TerrainRenderer = terrainRenderer;
        this.Presenter = presenter;
    }

    public void Resize(int width, int height)
    {
        this.GBuffer.Resize(width, height);
        this.UserInterface.Resize(width, height);
        this.CameraController.Resize(width, height);
    }

    public void Update(float elapsedSimulationTime)
    {
        this.UserInterface.NewFrame();
    }

    public void Draw(float alpha, float elapsedRealWorldTime)
    {
        this.CameraController.Update(elapsedRealWorldTime, this.Device.Viewport);

        this.GBuffer.Clear();

        var transform = this.CameraController.Transform;
        var output = this.Device.Viewport;

        this.Device.ImmediateContext.OM.SetRenderTargets(this.GBuffer.Depth, this.GBuffer.Albedo);
        this.TerrainRenderer.Render(this.Device.ImmediateContext, this.CameraController.Camera, in transform, in output, in output);

        this.Device.ImmediateContext.OM.SetRenderTargetToBackBuffer();
        // TODO: tone map later when we incorporate lights
        this.Presenter.Present(this.Device.ImmediateContext, this.GBuffer.Albedo);

        this.UserInterface.Render();
    }

    public void Dispose()
    {
        this.GBuffer.Dispose();
    }
}
