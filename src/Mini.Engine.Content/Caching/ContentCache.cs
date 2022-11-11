using Mini.Engine.Core.Lifetime;

namespace Mini.Engine.Content.Caching;

public sealed class ContentCache<T> : IContentCache<ILifetime<T>>
    where T : IDisposable
{
    private readonly LifetimeManager LifetimeManager;
    private readonly Dictionary<ContentId, ILifetime<T>> Cache;

    public ContentCache(LifetimeManager lifetimeManager)
    {
        this.LifetimeManager = lifetimeManager;
        this.Cache = new Dictionary<ContentId, ILifetime<T>>();
    }

    public void Store(ContentId id, ILifetime<T> value)
    {
        this.Cache.Add(id, value);
    }
    
    public bool TryGetValue(ContentId id, out ILifetime<T> value)
    {
        if (this.Cache.TryGetValue(id, out var lifetime))
        {
            if (this.LifetimeManager.IsValid(lifetime))
            {
                value = lifetime;
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
