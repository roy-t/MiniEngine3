using System.Diagnostics;
using Mini.Engine.Content.v2.Serialization;
using Mini.Engine.IO;
using Serilog;

namespace Mini.Engine.Content.v2;

internal sealed class HotReloader
{
    private record ReloadReference(WeakReference<IContent> Content, IContentProcessor Manager);

    private readonly ILogger Logger;
    private readonly IVirtualFileSystem FileSystem;    
    private readonly List<ReloadReference> References;

    public HotReloader(ILogger logger, IVirtualFileSystem fileSystem)
    {
        this.Logger = logger.ForContext<HotReloader>();
        this.FileSystem = fileSystem;        
        this.References = new List<ReloadReference>();
    }

    [Conditional("DEBUG")]
    internal void Register(IContent content, IContentProcessor manager)
    {
        foreach (var dependency in content.Dependencies)
        {
            this.FileSystem.WatchFile(dependency);
        }

        var reference = new ReloadReference(new WeakReference<IContent>(content), manager);
        this.References.Add(reference);
    }

    [Conditional("DEBUG")]
    public void ReloadChangedContent()
    {
        foreach (var file in this.FileSystem.GetChangedFiles())
        {
            for (var i = this.References.Count - 1; i >= 0; i--)
            {
                var reference = this.References[i].Content;
                if (reference.TryGetTarget(out var content))
                {
                    if (content.Dependencies.Contains(file))
                    {
                        this.Logger.Information("Reloading {@type}:{@content} because it references {@file}", content.GetType().Name, content.Id.ToString(), file);

                        var path = PathGenerator.GetPath(content.Id);
                        using var rwStream = this.FileSystem.CreateWriteRead(path);
                        using var writerReader = new ContentWriterReader(rwStream);
                        
                        var trackingFileSystem = new TrackingVirtualFileSystem(this.FileSystem);
                        this.References[i].Manager.Reload(content, writerReader, trackingFileSystem);
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
