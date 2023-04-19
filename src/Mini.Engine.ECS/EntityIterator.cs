using Mini.Engine.ECS.Components;

namespace Mini.Engine.ECS;
public struct EntityIterator<T>
    where T : struct, IComponent
{
    private readonly ComponentPool<T> Pool;
    private readonly IQuery<T> Query;
    private int index;

    public EntityIterator(ComponentPool<T> pool, IQuery<T> query)
    {
        this.Pool = pool;
        this.Query = query;
        this.index = -1;
    }

    public Entity Current => this.Pool[this.index].Entity;

    public bool MoveNext()
    {
        while (this.index < this.Pool.Count - 1)
        {
            this.index++;

            ref var entry = ref this.Pool[this.index];
            if (this.Query.Accept(ref entry))
            {
                return true;
            }
        }

        return false;
    }

    public EntityIterator<T> GetEnumerator()
    {
        return this;
    }
}
