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
    private readonly ILogger Logger;
    private readonly IVirtualFileSystem FileSystem;
    private readonly Device Device;

    private readonly Stack<List<IContent>> ContentStack;

    private readonly ContentCache<Texture2DContent> TextureLoader;
    private readonly ContentCache<MaterialContent> MaterialLoader;
    private readonly ContentCache<ModelContent> ModelLoader;

    public ContentManager(ILogger logger, Device device, IVirtualFileSystem fileSystem)
    {
        this.Logger = logger.ForContext<ContentManager>();
        this.ContentStack = new Stack<List<IContent>>();
        this.ContentStack.Push(new List<IContent>());
        this.Device = device;
        this.FileSystem = fileSystem;

        this.TextureLoader = new ContentCache<Texture2DContent>(new TextureLoader(this, fileSystem));
        this.MaterialLoader = new ContentCache<MaterialContent>(new MaterialLoader(this, fileSystem, this.TextureLoader));
        this.ModelLoader = new ContentCache<ModelContent>(new ModelLoader(this, fileSystem, this.MaterialLoader));
    }

    public IModel LoadSponza()
    {
        return this.ModelLoader.Load(this.Device, new ContentId(@"Scenes\sponza\sponza.obj"));
    }

    public IModel LoadAsteroid()
    {
        return this.ModelLoader.Load(this.Device, new ContentId(@"Scenes\AsteroidField\Asteroid001.obj"));
    }

    public void Push()
    {
        this.ContentStack.Push(new List<IContent>());
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

    internal void Add(IContent content)
    {
        this.Track(content);
        this.ContentStack.Peek().Add(content);
    }

    [Conditional("DEBUG")]
    internal void Track(IContent content)
    {
        this.FileSystem.WatchFile(content.Id.Path);
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
        foreach (var list in this.ContentStack)
        {
            foreach (var content in list)
            {
                if (content.Id.Path.Equals(path, StringComparison.OrdinalIgnoreCase))
                {
                    this.Logger.Information("Reloading {@content} because it references {@file}", content.GetType().Name, path);
                    content.Reload(this.Device);
                }
            }
        }
    }

    private void Unload(List<IContent> list)
    {
        foreach (var content in list)
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
                case PixelShaderContent ps:
                    ps.Dispose();
                    break;
                case VertexShaderContent vs:
                    vs.Dispose();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(content), $"Cannot unload {content.Id}, unsupported content type {content.GetType()}");
            }
        }
    }
}
