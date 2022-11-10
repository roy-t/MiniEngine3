using Mini.Engine.Content.v2.Serialization;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.IO;

namespace Mini.Engine.Content.v2;

public abstract class ContentProcessor<TContent, TWrapped, TSettings> : IContentProcessor<TContent, TWrapped, TSettings>
    where TContent : class
    where TWrapped : IContent<TContent, TSettings>, TContent
{
    protected ContentProcessor(int version, Guid type, params string[] supportedExtensions)
    {
        this.Version = version;
        this.Type = type;
        this.SupportedExtensions = new HashSet<string>(supportedExtensions);
    }

    public int Version { get; }
    public Guid Type { get; }
    public IReadOnlySet<string> SupportedExtensions { get; }

    public void Generate(ContentId id, TSettings settings, ContentWriter writer, TrackingVirtualFileSystem fileSystem)
    {
        if (this.HasSupportedExtension(id.Path))
        {
            using var bodyStream = new MemoryStream();

            var bodyWriter = new ContentWriter(bodyStream);
            this.WriteSettings(id, settings, bodyWriter);
            this.WriteBody(id, settings, bodyWriter, fileSystem);

            writer.WriteHeader(this.Type, this.Version, fileSystem.GetDependencies());
            bodyStream.WriteTo(writer.Writer.BaseStream);
        }
        else
        {
            throw new NotSupportedException($"Unsupported extension {id}");
        }
    }

    protected abstract void WriteSettings(ContentId id, TSettings settings, ContentWriter writer);
    protected abstract void WriteBody(ContentId id, TSettings settings, ContentWriter writer, IReadOnlyVirtualFileSystem fileSystem);


    public TContent Load(ContentId id, ContentHeader header, ContentReader reader)
    {
        ContentProcessorUtilities.ValidateHeader(this.Type, this.Version, header);

        var settings = this.ReadSettings(id, reader);
        return this.ReadBody(id, settings, reader);
    }

    protected abstract TSettings ReadSettings(ContentId id, ContentReader reader);
    protected abstract TContent ReadBody(ContentId id, TSettings settings, ContentReader reader);

    public void Reload(IContent original, ContentWriterReader writerReader, TrackingVirtualFileSystem fileSystem)
    {
        ContentReloader.Reload(this, (TWrapped)original, fileSystem, writerReader);
    }

    public abstract TWrapped Wrap(ContentId id, TContent content, TSettings settings, ISet<string> dependencies);

    public bool HasSupportedExtension(string path)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();
        return this.SupportedExtensions.Contains(extension);
    }
}


public abstract class UnmanagedContentProcessor<TContent, TWrapped, TSettings>
    : ContentProcessor<TContent, TWrapped, TSettings>, IUnmanagedContentProcessor<TContent, TWrapped, TSettings>
    where TContent : class, IDisposable
    where TWrapped : IContent<TContent, TSettings>, TContent
{
    public UnmanagedContentProcessor(int version, Guid type, params string[] supportedExtensions)
        : base(version, type, supportedExtensions)
    {
        this.Cache = new ContentTypeCache<ILifetime<TContent>>();
    }

    public IContentTypeCache<ILifetime<TContent>> Cache { get; }
}

public abstract class ManagedContentProcessor<TContent, TWrapped, TSettings>
    : ContentProcessor<TContent, TWrapped, TSettings>, IManagedContentProcessor<TContent, TWrapped, TSettings>
    where TContent : class
    where TWrapped : IContent<TContent, TSettings>, TContent
{
    public ManagedContentProcessor(int version, Guid type, params string[] supportedExtensions)
        : base(version, type, supportedExtensions)
    {
        this.Cache = new ContentTypeCache<TContent>();
    }

    public IContentTypeCache<TContent> Cache { get; }
}

public static class ContentProcessorUtilities
{
    public static void ValidateHeader(Guid expectedType, int expectedVersion, ContentHeader actual)
    {
        if (expectedType != actual.Type)
        {
            throw new NotSupportedException($"Unexpected type, expected: {expectedType}, actual: {actual.Type}");
        }

        if (expectedVersion != actual.Version)
        {
            throw new NotSupportedException($"Unexpected version, expected: {expectedVersion}, actual: {actual.Version}");
        }
    }

    public static bool IsContentUpToDate(int expectedVersion, ContentHeader header, IVirtualFileSystem fileSystem)
    {
        if (header.Version != expectedVersion)
        {
            return false;
        }

        var lastWrite = header.Dependencies
            .Select(d => fileSystem.GetLastWriteTime(d))
            .Append(header.Timestamp).Max();

        return lastWrite <= header.Timestamp;
    }
}
