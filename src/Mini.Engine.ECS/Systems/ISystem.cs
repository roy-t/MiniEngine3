namespace Mini.Engine.ECS.Systems;

public interface ISystem : ISystemBindingProvider
{
    public void OnSet();
    public void OnUnSet();
}
