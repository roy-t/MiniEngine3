using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.Graphics.PostProcessing;
using Mini.Engine.Titan.Graphics;
using Mini.Engine.UI;

namespace Mini.Engine.Titan;

[Service]
internal class TitanGameLoop : IGameLoop
{
    private readonly Device Device;
    private readonly ContentManager Content;
    private readonly EditorUserInterface UserInterface;
    private readonly PresentationHelper Presenter;
    private readonly GBuffer GBuffer;
    private readonly StrategyCameraController CameraController;
    private readonly TerrainRenderer TerrainRenderer;
    private readonly Terrain Terrain;

    public TitanGameLoop(Device device, ContentManager content, EditorUserInterface userInterface, PresentationHelper presenter, StrategyCameraController cameraController, Terrain terrainRenderer, TerrainRenderer terrainPartRenderer)
    {
        this.GBuffer = new GBuffer(device, MultiSamplingRequest.Eight);

        this.Device = device;
        this.Content = content;
        this.UserInterface = userInterface;
        this.CameraController = cameraController;
        this.Terrain = terrainRenderer;
        this.Presenter = presenter;
        this.TerrainRenderer = terrainPartRenderer;
    }

    public void Resize(int width, int height)
    {
        this.GBuffer.Resize(width, height);
        this.UserInterface.Resize(width, height);
        this.CameraController.Resize(width, height);
    }

    public void Update(float elapsedSimulationTime)
    {
        this.Content.ReloadChangedContent();
    }

    public void Draw(float alpha, float elapsedRealWorldTime)
    {
        this.UserInterface.NewFrame();

        this.CameraController.Update(elapsedRealWorldTime, this.Device.Viewport);

        this.GBuffer.Clear();

        var transform = this.CameraController.Transform;
        var output = this.Device.Viewport;

        this.Device.ImmediateContext.OM.SetRenderTargets(this.GBuffer.Group, this.GBuffer.Depth);
        this.TerrainRenderer.Setup(this.Device.ImmediateContext, this.CameraController.Camera, in transform);
        this.TerrainRenderer.Render(this.Device.ImmediateContext, in output, in output, this.Terrain);

        this.Device.ImmediateContext.OM.SetRenderTargetToBackBuffer();
        // TODO: tone map later when we incorporate lights
        if (this.GBuffer.Albedo.Sampling.Count > 1)
        {
            this.Presenter.PresentMultiSampled(this.Device.ImmediateContext, this.GBuffer.Albedo, this.GBuffer.Albedo.Sampling.Count);
        }
        else
        {
            this.Presenter.Present(this.Device.ImmediateContext, this.GBuffer.Albedo);
        }

        this.UserInterface.Render();
    }

    public void Dispose()
    {
        this.GBuffer.Dispose();
    }
}
