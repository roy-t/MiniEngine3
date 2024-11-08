﻿using System.Diagnostics;
using Mini.Engine.Content.Serialization;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.IO;
using Serilog;

namespace Mini.Engine.Content;

internal sealed class HotReloader
{
    private record ReloadReference(ILifetime<IDisposable> Content, IContentProcessor Manager, IList<Action> Callbacks);

    private readonly ILogger Logger;
    private readonly IVirtualFileSystem FileSystem;
    private readonly List<ReloadReference> References;
    private readonly List<Action<ContentId, Exception?>> Reporters;
    private readonly LifetimeManager LifetimeManager;

    public HotReloader(LifetimeManager lifetimeManager, ILogger logger, IVirtualFileSystem fileSystem)
    {
        this.LifetimeManager = lifetimeManager;
        this.Logger = logger.ForContext<HotReloader>();
        this.FileSystem = fileSystem;
        this.References = new List<ReloadReference>(0);
        this.Reporters = new List<Action<ContentId, Exception?>>(0);
    }

    [Conditional("DEBUG")]
    internal void Register(ILifetime<IDisposable> contentLifetime, IContentProcessor manager)
    {
        var content = (IContent)this.LifetimeManager.Get(contentLifetime);
        foreach (var dependency in content.Dependencies)
        {
            this.FileSystem.WatchFile(dependency);
        }

        var reference = new ReloadReference(contentLifetime, manager, new List<Action>(0));
        this.References.Add(reference);
    }

    [Conditional("DEBUG")]
    public void AddReloadCallback(ContentId id, Action callback)
    {
        for (var i = this.References.Count - 1; i >= 0; i--)
        {
            var reference = this.References[i];
            if (this.LifetimeManager.IsValid(reference.Content))
            {
                var content = (IContent)this.LifetimeManager.Get(reference.Content);
                if (content.Id == id)
                {
                    reference.Callbacks.Add(callback);
                    return;
                }
            }
            else
            {
                this.References.RemoveAt(i);
            }
        }

        throw new KeyNotFoundException($"Could not find {id}");
    }

    [Conditional("DEBUG")]
    public void AddReloadReporter(Action<ContentId, Exception?> callback)
    {
        this.Reporters.Add(callback);
    }

    [Conditional("DEBUG")]
    public void ReloadChangedContent()
    {
        foreach (var file in this.FileSystem.GetChangedFiles())
        {
            for (var i = this.References.Count - 1; i >= 0; i--)
            {
                var reference = this.References[i];
                if (this.LifetimeManager.IsValid(reference.Content))
                {
                    var content = (IContent)this.LifetimeManager.Get(reference.Content);
                    if (content.Dependencies.Contains(file))
                    {
                        this.Logger.Information("Reloading {@type}:{@content} because of changes in {@file}", content.GetType().Name, content.Id.ToString(), file);

                        try
                        {

                            var path = PathGenerator.GetPath(content.Id);
                            using var rwStream = this.FileSystem.CreateWriteRead(path);
                            using var writerReader = new ContentWriterReader(rwStream);

                            var trackingFileSystem = new TrackingVirtualFileSystem(this.FileSystem);
                            this.References[i].Manager.Reload(content, writerReader, trackingFileSystem);

                            foreach (var callback in reference.Callbacks)
                            {
                                callback();
                            }

                            foreach (var callback in this.Reporters)
                            {
                                callback(content.Id, null);
                            }
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Error(ex, "Reloading failed");

                            foreach (var callback in this.Reporters)
                            {
                                callback(content.Id, ex);
                            }
                        }
                    }
                }
                else
                {
                    this.References.RemoveAt(i);
                }
            }
        }
    }
}
