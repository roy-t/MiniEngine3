using Mini.Engine.Configuration;
using Mini.Engine.ECS.Components;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.ECS.Systems;

namespace Mini.Engine.Graphics.Transforms;

[System]
public sealed partial class TransformSystem : ISystem
{
    private readonly IComponentContainer<TransformComponent> Transforms;

    public TransformSystem(IComponentContainer<TransformComponent> transforms)
    {
        this.Transforms = transforms;
    }

    public void OnSet() { }

    public void Run()
    {
        foreach (ref var transform in this.Transforms.IterateAll())
        {
            transform.Value.Previous = transform.Value.Current;
        }
    }

    [Process(Query = ProcessQuery.All)]
    public void Process(ref TransformComponent component)
    {
        component.Previous = component.Current;
    }

    public void OnUnSet() { }
}
