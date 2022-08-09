using Mini.Engine.Configuration;

namespace Mini.Engine.ECS.Components;

[Component]
public interface IComponent
{
    public Entity Entity { get; set; }
    public LifeCycle LifeCycle { get; set; }
    public void Destroy();
}
