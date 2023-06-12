using LibGame.Collections;
using Mini.Engine.ECS.Components;

namespace Mini.Engine.ECS;
public readonly struct EntityIterator<T>
    where T : struct, IComponent
{
    private readonly IStructEnumerator<Component<T>> Enumerator;
    private readonly IQuery<T> Query;

    public EntityIterator(StructPool<Component<T>> pool, IQuery<T> query)
    {
        this.Enumerator = pool.GetEnumerator();
        this.Query = query;
    }

    public readonly Entity Current => this.Enumerator.Current.Entity;

    public readonly bool MoveNext()
    {
        while (this.Enumerator.MoveNext())
        {
            if (this.Query.Accept(ref this.Enumerator.Current))
            {
                return true;
            }
        }

        return false;
    }

    public readonly EntityIterator<T> GetEnumerator()
    {
        return this;
    }
}
