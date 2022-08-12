using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;

namespace Mini.Engine.Graphics.Transforms;

public struct TransformComponent : IComponent
{
    // TODO: instread of ITransformable transform should be an immutable struct and create a copy of itself
    public Transform Transform { get; set; }
    public Entity Entity { get; set; }
    public LifeCycle LifeCycle { get; set; }

    public void Init()
    {
        
    }

    public void Destroy()
    {
        
    }

    public TransformComponent OnTransform()
    {
        return this;
    }
}
