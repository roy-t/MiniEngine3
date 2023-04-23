using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.ECS;
using Mini.Engine.Graphics;
using Mini.Engine.Graphics.Diesel;
using Mini.Engine.Graphics.PostProcessing;
using Mini.Engine.Graphics.Transforms;
using Mini.Engine.UI;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace Mini.Engine.Diesel;

[Service]
internal sealed class DieselGameLoop : IGameLoop
{
    private readonly Device Device;
    private readonly DieselUserInterface UserInterface;
    private readonly ECSAdministrator Administrator;
    private readonly DieselUpdateLoop UpdateLoop;
    private readonly DieselRenderLoop RenderLoop;

    private RenderTarget albedo; // TODO: replace with ILifeTime things!
    private DepthStencilBuffer depthStencilBuffer; // TODO: replace with ILifeTime things!

    private readonly PresentationHelper PresentationHelper;
    private readonly CameraService CameraService;

    private bool isSceneSet;

    public DieselGameLoop(Device device, DieselUserInterface userInterface, ECSAdministrator administrator, DieselUpdateLoop updateLoop, DieselRenderLoop renderLoop, PresentationHelper presentationHelper, CameraService cameraService)
    {
        this.Device = device;
        this.UserInterface = userInterface;
        this.Administrator = administrator;
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
        if (!this.isSceneSet)
        {
            this.isSceneSet = true;

            var entities = this.Administrator.Entities;
            var components = this.Administrator.Components;

            var entity = entities.Create();
            ref var transform = ref components.Create<TransformComponent>(entity);
            transform.Current = Transform.Identity;
            
            var positions = new Vector3[]
            {
                new Vector3(-1, 0, 0),
                new Vector3(0, 0, -1),
                new Vector3(1, 0, 0),
            };

            var vertices = new PrimitiveVertex[]
            {
                new PrimitiveVertex(positions[0], new Vector3(0, 1, 0)),
                new PrimitiveVertex(positions[1], new Vector3(0, 1, 0)),
                new PrimitiveVertex(positions[2], new Vector3(0, 1, 0)),
            };

            var indices = new int[] { 0, 1, 2, };
            var mesh = new PrimitiveMesh(this.Device, vertices, indices, BoundingBox.CreateFromPoints(positions), "mesh");

            ref var primitive = ref components.Create<PrimitiveComponent>(entity);
            primitive.Mesh = this.Device.Resources.Add(mesh);
            primitive.Color = Colors.Red;            
        }

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
