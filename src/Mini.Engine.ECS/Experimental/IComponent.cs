namespace Mini.Engine.ECS.Experimental;

[Mini.Engine.Configuration.Component]
public interface IComponent
{
    public Entity Entity { get; set; }
    public LifeCycle LifeCycle { get; set; }
    public void Destroy();
}
