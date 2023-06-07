using LibGame.Collections;

namespace Mini.Engine.ECS.Components;

public struct ResultIterator<T>
    where T : struct, IComponent
{
    private readonly IStructEnumerator<Component<T>> Enumerator;
    private readonly IQuery<T> Query;

    public ResultIterator(StructPool<Component<T>> pool, IQuery<T> query)
    {
        this.Enumerator = pool.GetEnumerator();
        this.Query = query;
    }

    public ref Component<T> Current => ref this.Enumerator.Current;

    public bool MoveNext()
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

    public ResultIterator<T> GetEnumerator()
    {
        return this;
    }
}
