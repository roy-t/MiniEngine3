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

    private readonly Dictionary<ContentId, Entry> Cache;
    private readonly IContentLoader<T> Loader;

    public ContentCache(IContentLoader<T> loader)
    {
        this.Cache = new Dictionary<ContentId, Entry>();
        this.Loader = loader;
    }

    public T Load(Device device, ContentId id)
    {
        if (this.Cache.TryGetValue(id, out var entry))
        {
            entry.ReferenceCount++;
            return entry.Item;
        }

        var data = this.Loader.Load(device, id);
        this.Cache.Add(id, new Entry(data) { ReferenceCount = 1 });

        return data;
    }

    public void Unload(T content)
    {
        var entry = this.Cache[content.Id];

        entry.ReferenceCount--;
        if (entry.ReferenceCount < 1)
        {
            this.Loader.Unload(content);
            this.Cache.Remove(content.Id);
        }
    }
}
