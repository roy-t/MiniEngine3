﻿namespace Mini.Engine.ECS.Components;

public struct ResultIterator<T>
    where T : struct, IComponent
{
    private readonly ComponentPool<T> Pool;
    private readonly IQuery<T> Query;
    private int index;

    public ResultIterator(ComponentPool<T> pool, IQuery<T> query)
    {
        this.Pool = pool;
        this.Query = query;
        this.index = -1;
    }

    public ref Component<T> Current => ref this.Pool[this.index];

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

    public ResultIterator<T> GetEnumerator()
    {
        return this;
    }
}
