using Mini.Engine.ECS;

namespace Mini.Engine.Graphics.Transforms;

public sealed class TransformComponent : Component, ITransformable<TransformComponent>
{
    public TransformComponent(Entity entity)
        : base(entity)
    {
        this.Transform = new Transform();
    }

    public Transform Transform { get; }

    public TransformComponent OnTransform()
    {
        return this;
    }
}
