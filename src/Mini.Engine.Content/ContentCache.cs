using System.Collections.Generic;
using Mini.Engine.DirectX;

namespace Mini.Engine.Content;

internal interface IContentLoader<T>
    where T : IContent
{
    T Load(Device device, string fileName);
}

// TODO: let TextureLoader implement IContentLoader<T>
// and then use the cached variant always
// because there are two kinds of textures we unfortunately need two texture loaders
// HDR and regular
internal sealed class ContentCache<T>
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

}
