namespace Mini.Engine.ECS.Components
{
    public interface IComponent
    {
        Entity Entity { get; }
        ComponentChangeState ChangeState { get; }
    }
}
