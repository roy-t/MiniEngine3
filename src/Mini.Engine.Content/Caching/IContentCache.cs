namespace Mini.Engine.Content.Caching;

public interface IContentCache<T>
    where T : class
{
    public bool TryGetValue(ContentId id, out T value);
    public void Store(ContentId id, T value);
}
