using Mini.Engine.Content.v2.Serialization;
using Mini.Engine.IO;
using Serilog;

namespace Mini.Engine.Content.v2;

internal sealed class HotReloader
{
    private readonly ILogger Logger;    
    private readonly IVirtualFileSystem FileSystem;
    private readonly Dictionary<string, IContentGenerator> Generators;
    private readonly List<WeakReference<IContent>> References;

    public HotReloader(ILogger logger, IVirtualFileSystem fileSystem, IReadOnlyList<IContentGenerator> generators)
    {
        this.Logger = logger.ForContext<HotReloader>();        
        this.FileSystem = fileSystem;
        this.Generators = generators.ToDictionary(x => x.GeneratorKey);
        this.References = new List<WeakReference<IContent>>();
    }

    public void Register(IContent content)
    {
        foreach (var dependency in content.Dependencies)
        {
            this.FileSystem.WatchFile(dependency);
        }

        this.References.Add(new WeakReference<IContent>(content));
    }

    public void ReloadChangedContent()
    {
        foreach (var file in this.FileSystem.GetChangedFiles())
        {
            for (var i = this.References.Count - 1; i >= 0; i--)
            {
                var reference = this.References[i];
                if (reference.TryGetTarget(out var content))
                {
                    if (content.Dependencies.Contains(file))
                    {
                        this.Logger.Information("Reloading {@content} because it references {@file}", content.GetType().Name, file);
                        var generator = this.Generators[content.GeneratorKey];

                        var path = content.Id.Path + Constants.Extension;
                        using var rwStream = this.FileSystem.CreateWriteRead(path);

                        var trackingFileSystem = new TrackingVirtualFileSystem(this.FileSystem);
                        generator.Reload(content, trackingFileSystem, rwStream);
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
