//namespace Mini.Engine.ECS.Experimental;

//public interface IQuery<T>
//    where T : struct, IComponent
//{
//    public bool Accept(ref T component);
//}

//public struct ResultIterator<T>
//    where T : struct, IComponent
//{
//    private readonly PoolAllocator<T> Pool;
//    private readonly IQuery<T> Query;
//    private int index;

//    public ResultIterator(PoolAllocator<T> pool, IQuery<T> query)
//    {
//        this.Pool = pool;
//        this.Query = query;
//        this.index = -1;
//    }

//    public ref T Current => ref this.Pool[this.index];

//    public bool MoveNext()
//    {
//        while (this.index < this.Pool.HighestUsedSlot)
//        {
//            this.index++;

//            if (this.Pool.IsOccupied(this.index))
//            {
//                ref var component = ref this.Pool[this.index];
//                if (this.Query.Accept(ref component))
//                {
//                    return true;
//                }
//            }
//        }

//        return false;
//    }
//}

//public sealed class ComponentContainer<T>
//    where T : struct, IComponent
//{
//    private const int InitialCapacity = 10;
//    private readonly PoolAllocator<T> Pool;

//    // TODO: how to lookup components by entity
//    // we could let the pool allocator do insertion sort on entity id?
//    // and then a binary search on the entity?
//    // then give components a bitset that tracks which components it has?

//    public ComponentContainer()
//    {
//        this.Pool = new PoolAllocator<T>(InitialCapacity);
//    }

//    public ref T Create(Entity entity)
//    {
//        return ref this.Pool.Create();
//    }

//    public void Destroy(int index)
//    {
//        this.Pool.Destroy(index);
//    }

//    public ResultIterator<T> Query(IQuery<T> query)
//    {
//        return new ResultIterator<T>(this.Pool, query);
//    }
//}
