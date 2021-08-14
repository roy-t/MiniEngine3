using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.ECS.Systems;
using Vortice.Mathematics;

namespace Mini.Engine.Graphics;

[Service]
public partial class ClearGBufferSystem : ISystem
{
    private readonly Device Device;
    private readonly FrameService FrameService;

    public ClearGBufferSystem(Device device, FrameService frameService)
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
    public void Process() => this.Device.Clear(this.FrameService.GBuffer.Albedo, new Color(255, 0, 0));

}

