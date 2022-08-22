using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;

namespace Mini.Engine.Graphics.Vegetation;

public struct GrassComponent : IComponent
{
    public Entity Entity { get; set; }
    public LifeCycle LifeCycle { get; set; }
}
