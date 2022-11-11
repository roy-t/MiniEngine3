using Mini.Engine.Core.Lifetime;

namespace Mini.Engine.Content.Caching;

public sealed class UnmanagedContentCache<T> : IContentCache<ILifetime<T>>
    where T : class, IDisposable
{
    private readonly LifetimeManager LifetimeManager;
    private readonly Dictionary<ContentId, WeakReference<ILifetime<T>>> Cache;

    public UnmanagedContentCache(LifetimeManager lifetimeManager)
    {
        this.LifetimeManager = lifetimeManager;
        this.Cache = new Dictionary<ContentId, WeakReference<ILifetime<T>>>();
    }

    public void Store(ContentId id, ILifetime<T> value)
    {
        this.Cache.Add(id, new WeakReference<ILifetime<T>>(value));
    }

    public bool TryGetValue(ContentId id, out ILifetime<T> value)
    {
        if (this.Cache.TryGetValue(id, out var reference))
        {
            if (reference.TryGetTarget(out var target) && this.LifetimeManager.IsValid(target))
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
