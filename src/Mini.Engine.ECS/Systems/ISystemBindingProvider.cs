using Mini.Engine.ECS.Components;

namespace Mini.Engine.ECS.Systems;

public interface ISystemBindingProvider
{
    public Type GetSystemBindingType();    
}
