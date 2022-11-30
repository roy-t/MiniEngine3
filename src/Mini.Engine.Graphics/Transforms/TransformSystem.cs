using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.ECS.Systems;
using Vortice.Mathematics;

namespace Mini.Engine.Graphics.Transforms;

[System]
public sealed partial class TransformSystem : ISystem
{
    private readonly FrameService FrameService;

    public TransformSystem(FrameService frameService)
    {
        this.FrameService = frameService;
    }

    public void OnSet() { }

    [Process(Query = ProcessQuery.All)]
    public void Process(ref TransformComponent component)
    {
        component.Previous = component.Current;
    }

    [Process(Query = ProcessQuery.All)]
    public void Rotate(ref TransformComponent transform, ref MovementComponent movement)
    {
        var diff = this.FrameService.Elapsed * MathHelper.TwoPi * 0.5f;
        var rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, diff);
        //transform.Current = transform.Current.AddRotation(rotation);
    }

    public void OnUnSet() { }
}
