using System.Numerics;
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

    public void OnSet()
    {
        this.Device.ImmediateContext.OM.SetBlendState(this.Device.BlendStates.Opaque);
        this.Device.ImmediateContext.OM.SetDepthStencilState(this.Device.DepthStencilStates.None);
        this.Device.ImmediateContext.RS.SetRasterizerState(this.Device.RasterizerStates.CullNone);
    }

    [Process]
    public void Process()
    {
        // GBuffer
        this.Device.Clear(this.FrameService.GBuffer.Albedo, NeutralAlbedo);
        this.Device.Clear(this.FrameService.GBuffer.Material, NeutralMaterial);        
        this.Device.Clear(this.FrameService.GBuffer.Normal, NeutralNormal);

        this.Device.Clear(this.FrameService.GBuffer.DepthStencilBuffer,
             DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);

        // LBuffer
        this.Device.Clear(this.FrameService.LBuffer.Light, NeutralLight);
    }
    
    public void OnUnSet()
    {

    }
}

