﻿using Mini.Engine.Content.Caching;
using Mini.Engine.Content.Serialization;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.IO;

namespace Mini.Engine.Content;

public abstract class ContentProcessor<TContent, TWrapped, TSettings> : IContentProcessor<TContent, TWrapped, TSettings>
    where TContent : IDisposable
    where TWrapped : IContent<TContent, TSettings>, TContent
{
    protected ContentProcessor(LifetimeManager lifetimeManager, int version, Guid type, params string[] supportedExtensions)
    {
        this.Version = version;
        this.Type = type;
        this.SupportedExtensions = new HashSet<string>(supportedExtensions);

        this.Cache = new ContentCache<TContent>(lifetimeManager);
    }

    public IContentCache<ILifetime<TContent>> Cache { get; }

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

            foreach(var additionalDependency in this.GetAdditionalDependencies())
            {
                fileSystem.AddDependency(additionalDependency);
            }
                        
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

    protected virtual IEnumerable<string> GetAdditionalDependencies()
    {
        return Enumerable.Empty<string>();
    }

    public TContent Load(ContentId id, ContentHeader header, ContentReader reader)
    {
        ContentProcessorValidation.ThrowOnInvalidHeader(this.Type, this.Version, header);

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