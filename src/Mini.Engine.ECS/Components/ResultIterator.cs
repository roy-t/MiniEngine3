namespace Mini.Engine.ECS.Components;

public struct ResultIterator<T>
    where T : struct, IComponent
{
    private readonly PoolAllocator<T> Pool;
    private readonly IQuery<T> Query;
    private int index;

    public ResultIterator(PoolAllocator<T> pool, IQuery<T> query)
    {
        this.Pool = pool;
        this.Query = query;
        this.index = -1;
    }

    public ref T Current => ref this.Pool[this.index];

    public bool MoveNext()
    {
        while (this.index < this.Pool.Count - 1)
        {
            this.index++;

            ref var component = ref this.Pool[this.index];
            if (this.Query.Accept(ref component))
            {
                return true;
            }
        }

        return false;
    }
}
