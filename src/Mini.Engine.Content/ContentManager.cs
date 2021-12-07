using System;
using System.Collections.Generic;
using System.Diagnostics;
using Mini.Engine.Configuration;
using Mini.Engine.Content.Models;
using Mini.Engine.Content.Textures;
using Mini.Engine.DirectX;
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
    private readonly ContentCache<ModelContent> ModelLoader;

    public ContentManager(ILogger logger, Device device, IVirtualFileSystem fileSystem)
    {
        this.Logger = logger.ForContext<ContentManager>();
        this.ContentStack = new Stack<List<IContent>>();
        this.ContentStack.Push(new List<IContent>());
        this.Device = device;
        this.FileSystem = fileSystem;

        this.TextureLoader = new ContentCache<Texture2DContent>(new TextureLoader(fileSystem));
        this.ModelLoader = new ContentCache<ModelContent>(new ModelLoader(fileSystem, this.TextureLoader));
    }

    public Model LoadAsteroid()
    {
        var model = this.ModelLoader.Load(this.Device, @"Models\sponza\sponza.obj");
        this.Add(model);

        return model;
    }

    public void Push()
    {
        this.ContentStack.Push(new List<IContent>());
    }

    public void Pop()
    {
        this.Dispose(this.ContentStack.Pop());
    }

    public void Dispose()
    {
        while (this.ContentStack.Count > 0)
        {
            this.Dispose(this.ContentStack.Pop());
        }
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

        this.FileSystem.ClearChangedFiles();
    }

    private void Add(IContent content)
    {
        this.ContentStack.Peek().Add(content);
        this.Watch(content);
    }

    [Conditional("DEBUG")]
    private void Watch(IContent content)
    {
        this.FileSystem.WatchFile(content.FileName);
        this.Logger.Information("Watching file {@file}", content.FileName);
    }

    private void ReloadContentReferencingFile(string path)
    {
        foreach (var list in this.ContentStack)
        {
            foreach (var content in list)
            {
                if (content.FileName.Equals(path, StringComparison.OrdinalIgnoreCase))
                {
                    this.Logger.Information("Reloading {@content} because it references {@file}", content.GetType().Name, path);
                    content.Reload(this.Device);
                }
            }
        }
    }

    private void Dispose(List<IContent> list)
    {
        foreach (var content in list)
        {
            switch (content)
            {
                case Texture2DContent texture:
                    this.TextureLoader.Unload(texture);
                    break;
                case ModelContent model:
                    this.ModelLoader.Unload(model);
                    break;
            }
        }
    }
}
