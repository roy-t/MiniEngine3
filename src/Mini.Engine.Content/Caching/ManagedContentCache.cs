namespace Mini.Engine.Content.Caching;

public sealed class ManagedContentCache<T> : IContentCache<T>
    where T : class
{

    private readonly Dictionary<ContentId, WeakReference<T>> Cache;

    public ManagedContentCache()
    {
        this.Cache = new Dictionary<ContentId, WeakReference<T>>();
    }

    public void Store(ContentId id, T value)
    {
        this.Cache.Add(id, new WeakReference<T>(value));
    }

    public bool TryGetValue(ContentId id, out T value)
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
}
