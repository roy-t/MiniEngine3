using System.Collections.Generic;

namespace Mini.Engine.Content;

// TODO: let TextureLoader implement IContentLoader<T>
// and then use the cached variant always
// because there are two kinds of textures we unfortunately need two texture loaders
// HDR and regular
internal sealed class ContentCache<T> : IContentLoader<T>
    where T : IContentData
{
    private sealed record Entry(T Item)
    {
        public int ReferenceCount { get; set; }
    }

    private readonly Dictionary<string, Entry> Cache;
    private readonly IContentLoader<T> Loader;

    public ContentCache(IContentLoader<T> loader)
    {
        this.Cache = new Dictionary<string, Entry>();
        this.Loader = loader;
    }

    public T Load(string name)
    {
        var key = name.ToLowerInvariant();

        if (this.Cache.TryGetValue(key, out var entry))
        {
            entry.ReferenceCount++;
            return entry.Item;
        }

        var data = this.Loader.Load(name);
        this.Cache.Add(key, new Entry(data) { ReferenceCount = 1 });

        return data;
    }

}
