namespace Mini.Engine.ECS.Experimental;

public readonly record struct ComponentHash(long Hash)
{

    // TODO: use something like in ContainerStore to keep track of known components and their ids
    // make sure to make it thread safe!

    //public static ComponentHash Add<T>()
    //    where T : IComponent
    //{
        
    //}
}