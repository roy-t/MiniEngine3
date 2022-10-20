using Mini.Engine.Content.v2.Serialization;
using Mini.Engine.DirectX;
using Mini.Engine.IO;

namespace Mini.Engine.Content.v2;
internal sealed class ContentCache<T>
    where T : IContent
{
    private sealed record Entry(T Content)
    {
        public int ReferenceCount { get; set; }
    }

    private readonly Device Device;
    private readonly IVirtualFileSystem FileSystem;
    private readonly IContentGenerator<T> Generator;
    private readonly Dictionary<ContentId, Entry> Cache;

    public ContentCache(Device device, IContentGenerator<T> generator, IVirtualFileSystem fileSystem)
    {
        this.Device = device;
        this.Generator = generator;
        this.FileSystem = fileSystem;
        this.Cache = new Dictionary<ContentId, Entry>();
    }

    public T Load(ContentId id, ContentRecord meta)
    {
        var content = this.Get(id);
        if (content != null) { return content; }

        var path = id.Path + Constants.Extension;
        var blob = this.LoadFromFile(path) ?? this.Generate(id, meta, path);
        content = this.Generator.Load(this.Device, id, blob);

        this.Cache.Add(id, new Entry(content));
        return content;
    }

    public void Unload(ContentId id)
    {
        var entry = this.Cache[id];

        entry.ReferenceCount--;
        if (entry.ReferenceCount < 1)
        {
            entry.Content.Dispose();
            this.Cache.Remove(id);
        }
    }

    private T? Get(ContentId id)
    {
        if (this.Cache.TryGetValue(id, out var entry))
        {
            entry.ReferenceCount++;
            return entry.Content;
        }

        return default;
    }

    private ContentBlob? LoadFromFile(string path)
    {
        if (this.FileSystem.Exists(path))
        {
            using var rStream = this.FileSystem.OpenRead(path);
            var blob = ContentReader.ReadAll(rStream);
            if (this.IsCurrent(blob))
            {
                return blob;
            }
        }

        return null;
    }

    private bool IsCurrent(ContentBlob blob)
    {
        var lastWrite = blob.Dependencies
            .Select(d => this.FileSystem.GetLastWriteTime(d))
            .Append(blob.Timestamp).Max();

        return lastWrite <= blob.Timestamp;
    }

    private ContentBlob Generate(ContentId id, ContentRecord meta, string path)
    {
        using var rwStream = this.FileSystem.CreateWriteRead(path);
        var tracker = new TrackingVirtualFileSystem(this.FileSystem);
        this.Generator.Generate(id, meta, tracker, rwStream);

        rwStream.Seek(0, SeekOrigin.Begin);
        return ContentReader.ReadAll(rwStream);
    }
}
