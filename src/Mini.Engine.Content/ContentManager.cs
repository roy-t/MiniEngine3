using System.Diagnostics;
using Mini.Engine.Configuration;
using Mini.Engine.Content.Materials;
using Mini.Engine.Content.Models;
using Mini.Engine.Content.Shaders;
using Mini.Engine.Content.Textures;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Models;
using Mini.Engine.DirectX.Resources.Shaders;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.IO;
using Serilog;

namespace Mini.Engine.Content;

[Service]
public sealed class ContentManager
{
    private readonly ContentLoader Loader;

    private readonly SdrTextureProcessor SdrTextureProcessor;
    private readonly HdrTextureProcessor HdrTextureProcessor;
    private readonly ComputeShaderProcessor ComputeShaderProcessor;
    private readonly VertexShaderProcessor VertexShaderProcessor;
    private readonly PixelShaderProcessor PixelShaderProcessor;
    private readonly WavefrontMaterialProcessor MaterialProcessor;
    private readonly WaveFrontModelProcessor ModelProcessor;

    public ContentManager(ILogger logger, Device device, LifetimeManager lifetimeManager, IVirtualFileSystem fileSystem)
    {
        this.Loader = new ContentLoader(logger, lifetimeManager, fileSystem);

        this.SdrTextureProcessor = new SdrTextureProcessor(device);
        this.HdrTextureProcessor = new HdrTextureProcessor(device);
        this.ComputeShaderProcessor = new ComputeShaderProcessor(device);
        this.VertexShaderProcessor = new VertexShaderProcessor(device);
        this.PixelShaderProcessor = new PixelShaderProcessor(device);
        this.MaterialProcessor = new WavefrontMaterialProcessor(device, this);
        this.ModelProcessor = new WaveFrontModelProcessor(device, this);
    }

    public ILifetime<ITexture> LoadTexture(string path, TextureSettings settings)
    {
        return this.LoadTexture(new ContentId(path), settings);
    }

    public ILifetime<ITexture> LoadTexture(ContentId id, TextureSettings settings)
    {
        if (this.SdrTextureProcessor.HasSupportedExtension(id.Path))
        {
            return this.Load(this.SdrTextureProcessor, id, settings);
        }
        else if (this.HdrTextureProcessor.HasSupportedExtension(id.Path))
        {
            return this.Load(this.HdrTextureProcessor, id, settings);
        }

        throw new NotSupportedException($"No texture processor found that supports file {id.Path}");
    }

    public ILifetime<IMaterial> LoadMaterial(ContentId id, MaterialSettings settings)
    {
        return this.Load(this.MaterialProcessor, id, settings);
    }

    public ILifetime<IMaterial> LoadDefaultMaterial()
    {
        var settings = new MaterialSettings
        (
            TextureSettings.Default,
            TextureSettings.RenderData,
            TextureSettings.NormalMaps,
            TextureSettings.RenderData,
            TextureSettings.RenderData
        );

        var id = new ContentId("default.mtl", "default");

        return this.Load(this.MaterialProcessor, id, settings);
    }

    public ILifetime<IModel> LoadModel(string path, ModelSettings settings)
    {
        return this.Load(this.ModelProcessor, new ContentId(path), settings);
    }

    public ILifetime<IModel> LoadModel(ContentId id, ModelSettings settings)
    {
        return this.Load(this.ModelProcessor, id, settings);
    }

    public ILifetime<IComputeShader> LoadComputeShader(ContentId id, int numThreadsX, int numThreadsY, int numThreadsZ)
    {
        return this.Load(this.ComputeShaderProcessor, id, new ComputeShaderSettings(numThreadsX, numThreadsY, numThreadsZ));
    }

    public ILifetime<IVertexShader> LoadVertexShader(ContentId id)
    {
        return this.Load(this.VertexShaderProcessor, id, VertexShaderSettings.Empty);
    }

    public ILifetime<IPixelShader> LoadPixelShader(ContentId id)
    {
        return this.Load(this.PixelShaderProcessor, id, PixelShaderSettings.Empty);
    }

    public TContent Load<TContent, TWrapped, TSettings>(IManagedContentProcessor<TContent, TWrapped, TSettings> processor, ContentId id, TSettings settings)
        where TContent : class
        where TWrapped : IContent, TContent
    {
        return this.Loader.Load(processor, id, settings);
    }

    public ILifetime<TContent> Load<TContent, TWrapped, TSettings>(IUnmanagedContentProcessor<TContent, TWrapped, TSettings> processor, ContentId id, TSettings settings)
        where TContent : class, IDisposable
        where TWrapped : IContent, TContent
    {
        return this.Loader.Load(processor, id, settings);
    }

    public void ReloadChangedContent()
    {
        this.Loader.ReloadChangedContent();
    }
    
    public void AddReloadCallback(ContentId id, Action callback)
    {
        this.Loader.AddReloadCallback(id, callback);
    }
}
