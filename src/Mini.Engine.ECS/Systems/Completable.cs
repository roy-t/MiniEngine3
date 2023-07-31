namespace Mini.Engine.ECS.Systems;
public sealed class Completable : ICompletable
{
    public static readonly ICompletable Empty = new Completable();

    private Completable() { }    
    public void Complete() { }
}
