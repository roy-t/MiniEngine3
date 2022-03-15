using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
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

    public ClearBuffersSystem(Device device, FrameService frameService)
    {
        this.Device = device;
        this.FrameService = frameService;
    }

    public void OnSet() { }

    [Process]
    public void Process()
    {
        var context = this.Device.ImmediateContext;

        // GBuffer
        context.Clear(this.FrameService.GBuffer.Albedo, NeutralAlbedo);
        context.Clear(this.FrameService.GBuffer.Material, NeutralMaterial);
        context.Clear(this.FrameService.GBuffer.Normal, NeutralNormal);

        context.Clear(this.FrameService.GBuffer.DepthStencilBuffer,
             DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);

        // LBuffer
        context.Clear(this.FrameService.LBuffer.Light, NeutralLight);
    }

    public void OnUnSet() { }
}

