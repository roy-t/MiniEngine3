using Mini.Engine.Content.v2.Serialization;
using Mini.Engine.IO;

namespace Mini.Engine.Content.v2;
public sealed class ContentCache<T>
    where T : IContent
{
    private sealed record Entry(T Content)
    {
        public int ReferenceCount { get; set; }
    }

    private readonly IVirtualFileSystem FileSystem;
    private readonly IContentGenerator<T> Generator;
    private readonly Dictionary<ContentId, Entry> Cache;

    public ContentCache(IContentGenerator<T> generator, IVirtualFileSystem fileSystem)
    {
        this.Generator = generator;
        this.FileSystem = fileSystem;
        this.Cache = new Dictionary<ContentId, Entry>();
    }

    public T Load(ContentId id, ContentRecord meta)
    {
        var content = this.Get(id);
        if (content != null) { return content; }

        content = this.LoadFromFile(id) ?? this.Generate(id, meta);

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

    private T? LoadFromFile(ContentId id)
    {
        var path = id.Path + Constants.Extension;
        if (this.FileSystem.Exists(path))
        {
            using var stream = this.FileSystem.OpenRead(path);
            using var reader = new ContentReader(stream);
            var common = reader.ReadCommon();
            if (this.IsCurrent(common))
            {
                stream.Seek(0, SeekOrigin.Begin);
                return this.Generator.Load(id, reader);
            }
        }

        return default;
    }

    private bool IsCurrent(ContentBlob blob)
    {
        var lastWrite = blob.Dependencies
            .Select(d => this.FileSystem.GetLastWriteTime(d))
            .Append(blob.Timestamp).Max();

        return lastWrite <= blob.Timestamp;
    }

    private T Generate(ContentId id, ContentRecord meta)
    {
        var path = id.Path + Constants.Extension;

        using (var rwStream = this.FileSystem.CreateWriteRead(path))
        {
            using (var writer = new ContentWriter(rwStream))
            {
                var tracker = new TrackingVirtualFileSystem(this.FileSystem);
                this.Generator.Generate(id, meta, tracker, writer);
            }

            rwStream.Seek(0, SeekOrigin.Begin);

            using (var reader = new ContentReader(rwStream))
            {
                return this.Generator.Load(id, reader);
            }
        }
    }
}
