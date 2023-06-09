using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.ECS.Systems;
using Vortice.Direct3D11;
using Vortice.Mathematics;

namespace Mini.Engine.Graphics;

[Service]
public sealed partial class ClearBuffersSystem : ISystem
{
    private readonly Device Device;
    private readonly FrameService FrameService;

    private static readonly Color4 NeutralAlbedo = new Color4(0, 0, 0, 0);
    private static readonly Color4 NeutralMaterial = new Color4(0, 0, 0, 0.0f);
    private static readonly Color4 NeutralNormal = new Color4(0.0f, 0.0f, 0.0f, 0.0f);
    private static readonly Color4 NeutralLight = new Color4(0, 0, 0, 0.0f);
    private static readonly Color4 NeutralVelocity = new Color4(0, 0, 0, 0.0f);

    public ClearBuffersSystem(Device device, FrameService frameService)
    {
        this.Device = device;
        this.FrameService = frameService;
    }

    public static void Clear(DeviceContext context, FrameService frameService)
    {
        // GBuffer
        context.Clear(frameService.GBuffer.Albedo, NeutralAlbedo);
        context.Clear(frameService.GBuffer.Material, NeutralMaterial);
        context.Clear(frameService.GBuffer.Normal, NeutralNormal);
        context.Clear(frameService.GBuffer.Velocity, NeutralVelocity);

        context.Clear(frameService.GBuffer.DepthStencilBuffer,
             DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 0.0f, 0);

        // LBuffer
        context.Clear(frameService.LBuffer.Light, NeutralLight);
    }

    public void OnSet() { }

    [Process]
    public void Process()
    {
        var context = this.Device.ImmediateContext;
        Clear(context, this.FrameService);
    }

    public void OnUnSet() { }
}

