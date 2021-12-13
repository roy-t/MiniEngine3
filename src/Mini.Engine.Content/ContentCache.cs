using System.Collections.Generic;
using Mini.Engine.DirectX;

namespace Mini.Engine.Content;

internal sealed class ContentCache<T> : IContentLoader<T>
    where T : IContent
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

    public T Load(Device device, string fileName)
    {
        var key = fileName.ToLowerInvariant();

        if (this.Cache.TryGetValue(key, out var entry))
        {
            entry.ReferenceCount++;
            return entry.Item;
        }

        var data = this.Loader.Load(device, fileName);
        this.Cache.Add(key, new Entry(data) { ReferenceCount = 1 });

        return data;
    }

    public void Unload(T content)
    {
        var key = content.Id.ToLowerInvariant();
        var entry = this.Cache[key];

        entry.ReferenceCount--;
        if (entry.ReferenceCount < 1)
        {
            entry.Item.Dispose();
            this.Cache.Remove(key);
        }
    }
}
