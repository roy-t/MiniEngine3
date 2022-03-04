using System;
using System.Collections.Generic;
using System.Diagnostics;
using Mini.Engine.Configuration;
using Mini.Engine.Content.Materials;
using Mini.Engine.Content.Models;
using Mini.Engine.Content.Shaders;
using Mini.Engine.Content.Textures;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.IO;
using Serilog;

namespace Mini.Engine.Content;

[Service]
public sealed partial class ContentManager : IDisposable
{
    private sealed record Frame(string Name, List<IContent> Content)
    {
        public Frame(string name) : this(name, new List<IContent>()) { }
    }

    private readonly ILogger Logger;
    private readonly IVirtualFileSystem FileSystem;
    private readonly Device Device;

    private readonly Stack<Frame> ContentStack;

    private readonly ContentCache<Texture2DContent> TextureLoader;
    private readonly ContentCache<MaterialContent> MaterialLoader;
    private readonly ContentCache<ModelContent> ModelLoader;

    private readonly Dictionary<string, HashSet<ContentId>> Dependencies;

    public ContentManager(ILogger logger, Device device, IVirtualFileSystem fileSystem)
    {
        this.Logger = logger.ForContext<ContentManager>();
        this.ContentStack = new Stack<Frame>();
        this.ContentStack.Push(new Frame("Root"));
        this.Device = device;
        this.FileSystem = fileSystem;

        this.TextureLoader = new ContentCache<Texture2DContent>(new TextureLoader(this, fileSystem));
        this.MaterialLoader = new ContentCache<MaterialContent>(new MaterialLoader(this, fileSystem, this.TextureLoader));
        this.ModelLoader = new ContentCache<ModelContent>(new ModelLoader(this, fileSystem, this.MaterialLoader));

        this.Dependencies = new Dictionary<string, HashSet<ContentId>>(StringComparer.OrdinalIgnoreCase);
    }

    public ITexture2D LoadTexture(string path, string key = "")
    {
        return this.TextureLoader.Load(this.Device, new ContentId(path, key), TextureLoaderSettings.Default);
    }

    public IMaterial LoadMaterial(string path, string key = "")
    {
        return this.MaterialLoader.Load(this.Device, new ContentId(path, key), MaterialLoaderSettings.Default);
    }

    public IModel LoadModel(string path, string key = "")
    {
        return this.ModelLoader.Load(this.Device, new ContentId(path, key), ModelLoaderSettings.Default);
    }

    public IModel LoadSponza()
    {
        return this.LoadModel(@"Scenes\sponza\sponza.obj");        
    }

    public IModel LoadAsteroid()
    {
        return this.LoadModel(@"Scenes\AsteroidField\Asteroid001.obj");
    }

    public IModel LoadCube()
    {
        return this.LoadModel(@"Scenes\cube\cube.obj");
    }

    public IMaterial LoadDefaultMaterial()
    {
        return this.LoadMaterial("default.mtl", "default");
    }

    public void Push(string name)
    {
        this.Logger.Information("Creating content stack frame {@frame}", name);
        this.ContentStack.Push(new Frame(name));
    }

    public void Pop()
    {
        this.Unload(this.ContentStack.Pop());
    }

    public void Dispose()
    {
        while (this.ContentStack.Count > 0)
        {
            this.Unload(this.ContentStack.Pop());
        }
    }

    public void Link(IDisposable content, string id)
    {
        var wrapper = new ExternalContent(content, id);
        this.Add(wrapper);
    }

    internal void Add(IContent content)
    {
        this.Track(content);
        this.ContentStack.Peek().Content.Add(content);
    }    

    [Conditional("DEBUG")]
    private void Track(IContent content)
    {
        this.FileSystem.WatchFile(content.Id.Path);
        this.RegisterDependency(content.Id, content.Id.Path);
    }

    [Conditional("DEBUG")]
    internal void RegisterDependency(ContentId content, string file)
    {
        if (!this.Dependencies.TryGetValue(file, out var dependencies))
        {
            dependencies = new HashSet<ContentId>();
            this.Dependencies.Add(file, dependencies);
        }

        dependencies.Add(content);
        this.FileSystem.WatchFile(file);
    }

    [Conditional("DEBUG")]
    public void ReloadChangedContent()
    {
        foreach (var file in this.FileSystem.GetChangedFiles())
        {
            try
            {
                this.ReloadContentReferencingFile(file);
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex, "Failed to reload {@file}", file);
            }
        }
    }

    private void ReloadContentReferencingFile(string path)
    {
        if (this.Dependencies.TryGetValue(path, out var dependencies))
        {
            foreach (var frame in this.ContentStack)
            {
                foreach (var content in frame.Content)
                {
                    if (dependencies.Contains(content.Id))
                    {
                        this.Logger.Information("Reloading {@content} because it references {@file}", content.GetType().Name, path);
                        content.Reload(this.Device);
                    }
                }
            }
        }
    }

    private void Unload(Frame frame)
    {
        this.Logger.Information("Unloading content stack frame {@frame}", frame.Name);
        foreach (var content in frame.Content)
        {
            switch (content)
            {
                case Texture2DContent texture:
                    this.TextureLoader.Unload(texture);
                    break;
                case MaterialContent material:
                    this.MaterialLoader.Unload(material);
                    break;
                case ModelContent model:
                    this.ModelLoader.Unload(model);
                    break;
                case IShaderContent shader:
                    shader.Dispose();
                    break;               
                case ExternalContent external:
                    external.Dispose();
                    break;
                default:
                    throw new NotSupportedException($"Cannot unload {content.Id}, unsupported content type {content.GetType()}");
            }
        }
    }
}
