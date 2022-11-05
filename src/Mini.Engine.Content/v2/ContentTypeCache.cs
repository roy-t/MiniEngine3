using Mini.Engine.Core.Lifetime;

namespace Mini.Engine.Content.v2;
public sealed class ContentTypeCache<T> : IContentTypeCache<T>   
{

    private readonly Dictionary<ContentId, WeakReference<ILifetime<T>>> Cache;

    public ContentTypeCache()
    {
        this.Cache = new Dictionary<ContentId, WeakReference<ILifetime<T>>>();
    }

    public bool TryGetValue(ContentId id, out ILifetime<T> value)
    {
        if (this.Cache.TryGetValue(id, out var reference))
        {
            if (reference.TryGetTarget(out var target))
            {
                value = target;
                return true;
            }
            else
            {
                this.Cache.Remove(id);
            }
        }

        value = default!;
        return false;
    }

    public void Store(ContentId id, ILifetime<T> value)
    {
        this.Cache.Add(id, new WeakReference<ILifetime<T>>(value));
    }
}
