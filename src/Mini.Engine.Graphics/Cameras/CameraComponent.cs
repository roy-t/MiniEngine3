using System.Numerics;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;

namespace Mini.Engine.Graphics.Cameras;
public struct CameraComponent : IComponent
{
    public PerspectiveCamera Camera;
    public Vector2 Jitter;
    public Vector2 PreviousJitter;

    public Entity Entity { get; set; }
    public LifeCycle LifeCycle { get; set; }
}
