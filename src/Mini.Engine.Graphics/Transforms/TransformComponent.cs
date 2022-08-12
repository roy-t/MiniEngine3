using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;

namespace Mini.Engine.Graphics.Transforms;

public struct TransformComponent : IComponent
{    
    public Transform Transform;

    public Entity Entity { get; set; }
    public LifeCycle LifeCycle { get; set; }
}
