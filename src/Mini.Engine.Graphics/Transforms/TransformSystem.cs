using Mini.Engine.Configuration;
using Mini.Engine.ECS.Components;

namespace Mini.Engine.Graphics.Transforms;

[Service]
public sealed class TransformSystem
{
    private readonly IComponentContainer<TransformComponent> Transforms;

    public TransformSystem(IComponentContainer<TransformComponent> transforms)
    {
        this.Transforms = transforms;
    }

    public void Run()
    {
        foreach (ref var transform in this.Transforms.IterateAll())
        {
            transform.Value.Previous = transform.Value.Current;
        }
    }
}
