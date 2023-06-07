namespace Mini.Engine.ECS.Components;

public struct Component<T>
    where T : struct
{
    public T Value;
    public Entity Entity;
    public LifeCycle LifeCycle;
}

