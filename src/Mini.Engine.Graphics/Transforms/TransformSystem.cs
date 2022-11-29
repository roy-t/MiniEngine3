using Mini.Engine.Configuration;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.ECS.Systems;

namespace Mini.Engine.Graphics.Transforms;

[System]
public sealed partial class TransformSystem : ISystem
{
    public void OnSet() { }

    [Process(Query = ProcessQuery.All)]
    public void Process(ref TransformComponent component)
    {
        component.Previous = component.Current;
    }

    public void OnUnSet() { }
}
