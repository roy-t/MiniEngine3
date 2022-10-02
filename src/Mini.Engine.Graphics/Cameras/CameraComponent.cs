using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;

namespace Mini.Engine.Graphics.Cameras;
public struct CameraComponent : IComponent
{
    public PerspectiveCamera Camera;

    public Entity Entity { get; set; }
    public LifeCycle LifeCycle { get; set; }
}
