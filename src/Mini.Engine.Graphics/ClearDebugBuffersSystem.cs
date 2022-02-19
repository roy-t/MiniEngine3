using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.ECS.Systems;
using Vortice.Mathematics;

namespace Mini.Engine.Graphics;

[Service]
public sealed partial class ClearDebugBuffersSystem : ISystem
{
    private readonly Device Device;
    private readonly DebugFrameService FrameService;

    private static readonly Color4 Neutral = new Color4(0, 0, 0, 0);

    public ClearDebugBuffersSystem(Device device, DebugFrameService frameService)
    {
        this.Device = device;
        this.FrameService = frameService;
    }

    public void OnSet() { }

    [Process]
    public void Process()
    {
        this.Device.Clear(this.FrameService.DebugOverlay, Neutral);        
    }

    public void OnUnSet() { }
}

