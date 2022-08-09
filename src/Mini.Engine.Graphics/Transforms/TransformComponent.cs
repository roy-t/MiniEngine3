using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;

namespace Mini.Engine.Graphics.Transforms;

public struct TransformComponent : IComponent, ITransformable<TransformComponent>
{
    public void Init()        
    {
        this.Transform = new Transform();
    }

    public Transform Transform { get; private set; }
    public Entity Entity { get; set; }
    public LifeCycle LifeCycle { get; set; }

    public void Destroy()
    {
        
    }

    public TransformComponent OnTransform()
    {
        return this;
    }
}
