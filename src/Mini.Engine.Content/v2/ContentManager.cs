﻿using Mini.Engine.Configuration;
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
    private readonly VertexShaderProcessor VertexShaderProcessor;
    private readonly PixelShaderProcessor PixelShaderProcessor;

    public ContentManager(ILogger logger, Device device, LifetimeManager lifetimeManager, IVirtualFileSystem fileSystem)
    {
        this.LifetimeManager = lifetimeManager;
        this.FileSystem = fileSystem;
        this.HotReloader = new HotReloader(logger, fileSystem);

        this.SdrTextureProcessor = new SdrTextureProcessor(device);
        this.HdrTextureProcessor = new HdrTextureProcessor(device);
        this.ComputeShaderProcessor = new ComputeShaderProcessor(device);
        this.VertexShaderProcessor = new VertexShaderProcessor(device);
        this.PixelShaderProcessor = new PixelShaderProcessor(device);
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

    public ILifetime<IComputeShader> LoadComputeShader(ContentId id, int numThreadsX, int numThreadsY, int numThreadsZ)
    {
        return this.Load(this.ComputeShaderProcessor, new ComputeShaderSettings(numThreadsX, numThreadsY, numThreadsZ), id);        
    }

    public ILifetime<IVertexShader> LoadVertexShader(ContentId id)
    {
        return this.Load(this.VertexShaderProcessor, VertexShaderSettings.Empty, id);
    }

    public ILifetime<IPixelShader> LoadPixelShader(ContentId id)
    {
        return this.Load(this.PixelShaderProcessor, PixelShaderSettings.Empty, id);
    }

    public ILifetime<TContent> Load<TContent, TWrapped, TSettings>(IContentProcessor<TContent, TWrapped, TSettings> processor, TSettings settings, string path, string? key = null)
        where TContent : IDisposable
        where TWrapped : IContent, TContent
    {
        return this.Load(processor, settings, new ContentId(path, key ?? string.Empty));
    }

    public ILifetime<TContent> Load<TContent, TWrapped, TSettings>(IContentProcessor<TContent, TWrapped, TSettings> processor, TSettings settings, ContentId id)
        where TContent : IDisposable
        where TWrapped : IContent, TContent
    {
        // 1. Return existing reference        
        if (processor.Cache.TryGetValue(id, out var t))
        {
            return t;
        }

        // 2. Load from disk
        var path = PathGenerator.GetPath(id);
        if (this.FileSystem.Exists(path))
        {
            using var rStream = this.FileSystem.OpenRead(path);
            using var reader = new ContentReader(rStream);
            var header = reader.ReadHeader();
            if (ContentProcessor.IsContentUpToDate(processor, header, this.FileSystem))
            {
                var content = processor.Load(id, header, reader);
#if DEBUG
                var wrapped = processor.Wrap(id, content, settings, header.Dependencies);
                this.HotReloader.Register(wrapped, processor);
                var resource = this.RegisterContentResource((TContent)wrapped);
#else
                var resource = this.RegisterContentResource(content);                
#endif
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
        where T : IDisposable
    {
        return this.LifetimeManager.Add(content);
    }

    public void ReloadChangedContent()
    {
        this.HotReloader.ReloadChangedContent();
    }
}
