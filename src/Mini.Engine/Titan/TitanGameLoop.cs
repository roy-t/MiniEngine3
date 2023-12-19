using System.Numerics;
using ImGuiNET;
using LibGame.Physics;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.Graphics.Cameras;
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

    private PerspectiveCamera camera;

    public TitanGameLoop(Device device, UICore userInterface, PresentationHelper presenter, CameraController cameraController, TerrainRenderer terrainRenderer)
    {
        this.GBuffer = new GBuffer(device);        
        this.camera = new PerspectiveCamera(0.1f, 100.0f, MathF.PI / 2.0f, this.GBuffer.AspectRatio);

        this.Device = device;
        this.UserInterface = userInterface;
        this.CameraController = cameraController;
        this.TerrainRenderer = terrainRenderer;
        this.Presenter = presenter;
    }

    public void Resize(int width, int height)
    {
        this.UserInterface.Resize(width, height);
        this.GBuffer.Resize(width, height);
        this.camera = new PerspectiveCamera(0.1f, 100.0f, MathF.PI / 2.0f, this.GBuffer.AspectRatio);
    }

    public void Update(float elapsedSimulationTime, float elapsedRealWorldTime)
    {
        this.UserInterface.NewFrame(elapsedRealWorldTime);        

        this.CameraController.Update(elapsedRealWorldTime, in this.camera, this.Device.Viewport);
        ImGui.ShowDemoWindow();
    }

    public void Draw(float alpha, float elapsedRealWorldTime)
    {
        this.GBuffer.Clear();

        var transform = this.CameraController.GetCameraTransform();
        var output = this.Device.Viewport;

        this.Device.ImmediateContext.OM.SetRenderTargets(this.GBuffer.Depth, this.GBuffer.Albedo);
        this.TerrainRenderer.Render(this.Device.ImmediateContext, this.GBuffer, in this.camera, in transform, in output, in output);

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
