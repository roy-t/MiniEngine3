using Mini.Engine.Configuration;
using Mini.Engine.Content.Textures;
using Mini.Engine.Content.v2.Serialization;
using Mini.Engine.Content.v2.Shaders;
using Mini.Engine.Content.v2.Textures;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Shaders;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.IO;
using Serilog;

namespace Mini.Engine.Content.v2;

[Service]
public sealed class ContentManager
{
    private readonly LifetimeManager LifetimeManager;
    private readonly IVirtualFileSystem FileSystem;
    private readonly HotReloader HotReloader;

    private readonly SdrTextureProcessor SdrTextureProcessor;
    private readonly HdrTextureProcessor HdrTextureProcessor;
    private readonly ComputeShaderProcessor ComputeShaderProcessor;

    public ContentManager(ILogger logger, Device device, LifetimeManager lifetimeManager, IVirtualFileSystem fileSystem)
    {
        this.LifetimeManager = lifetimeManager;
        this.FileSystem = fileSystem;
        this.HotReloader = new HotReloader(logger, fileSystem);

        this.SdrTextureProcessor = new SdrTextureProcessor(device);
        this.HdrTextureProcessor = new HdrTextureProcessor(device);
        this.ComputeShaderProcessor = new ComputeShaderProcessor(device);
    }

    public ILifetime<ITexture> LoadTexture(string path, TextureLoaderSettings settings)
    {
        if (this.SdrTextureProcessor.HasSupportedSdrExtension(path))
        {
            return this.Load(this.SdrTextureProcessor, settings, path);
        }
        else if (this.HdrTextureProcessor.HasSupportedHdrExtension(path))
        {
            return this.Load(this.HdrTextureProcessor, settings, path);
        }

        throw new NotSupportedException($"No texture processor found that supports file {path}");
    }

    public ILifetime<IComputeShader> LoadComputeShader(string path, string key, int numThreadsX, int numThreadsY, int numThreadsZ)
    {
        return this.Load(this.ComputeShaderProcessor, new ComputeShaderSettings(numThreadsX, numThreadsY, numThreadsZ), path, key);
    }

    public ILifetime<TContent> Load<TContent, TSettings>(IContentProcessor<TContent, TSettings> processor, TSettings settings, string path, string? key = null)
        where TContent : IContent
    {
        return this.Load(processor, settings, new ContentId(path, key ?? string.Empty));
    }

    public ILifetime<TContent> Load<TContent, TSettings>(IContentProcessor<TContent, TSettings> processor, TSettings settings, ContentId id)
        where TContent : IContent
    {
        // 1. Return existing reference        
        if (processor.Cache.TryGetValue(id, out var t))
        {
            return t;
        }

        // 2. Load from disk
        var path = id.Path + Constants.Extension;
        if (this.FileSystem.Exists(path))
        {
            using var rStream = this.FileSystem.OpenRead(path);
            using var reader = new ContentReader(rStream);
            var header = reader.ReadHeader();
            if (Utilities.IsCurrent(processor, header, this.FileSystem))
            {
                var content = processor.Load(id, header, reader);
                this.HotReloader.Register(content, processor);

                var resource = this.RegisterContentResource(content);
                processor.Cache.Store(id, resource);

                return resource;
            }
        }

        // 3. Generate, store, load from disk                
        using (var rwStream = this.FileSystem.CreateWriteRead(path))
        {
            using var writer = new ContentWriter(rwStream);
            processor.Generate(id, settings, writer, new TrackingVirtualFileSystem(this.FileSystem));
        }

        return this.Load(processor, settings, id);
    }

    private ILifetime<T> RegisterContentResource<T>(T content)
        where T : IContent
    {
        return this.LifetimeManager.Add(content);
    }

    public void ReloadChangedContent()
    {
        this.HotReloader.ReloadChangedContent();
    }
}
