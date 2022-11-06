namespace Mini.Engine.Content.v2;
public sealed class ContentTypeCache<T> : IContentTypeCache<T>
    where T : class
{

    private readonly Dictionary<ContentId, WeakReference<T>> Cache;

    public ContentTypeCache()
    {
        this.Cache = new Dictionary<ContentId, WeakReference<T>>();
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

    public void Store(ContentId id, T value)
    {
        this.Cache.Add(id, new WeakReference<T>(value));
    }
}
