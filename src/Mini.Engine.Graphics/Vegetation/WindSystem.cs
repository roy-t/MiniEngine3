using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.ECS.Systems;

namespace Mini.Engine.Graphics.Vegetation;

[Service]
public sealed partial class WindSystem : ISystem
{
    private readonly FrameService FrameService;
    
    public WindSystem(FrameService frameService)
    {
        this.FrameService = frameService;
        this.Accumulator = 0.0f;
        this.Direction = Vector2.Normalize(new Vector2(1.0f, 0.75f));
    }

    public float Accumulator { get; private set; }

    public Vector2 Direction { get; private set; }

    public void OnSet() { }

    [Process]
    public void UpdateWind()
    {
        this.Accumulator += this.FrameService.Elapsed;
    }

    public void OnUnSet() { }
}
