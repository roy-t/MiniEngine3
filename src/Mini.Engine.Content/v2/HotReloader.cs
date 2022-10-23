using Mini.Engine.Content.v2.Serialization;
using Mini.Engine.IO;
using Serilog;

namespace Mini.Engine.Content.v2;

internal sealed class HotReloader
{
    private readonly ILogger Logger;
    private readonly ContentStack Stack;
    private readonly IVirtualFileSystem FileSystem;
    private readonly Dictionary<string, IContentGenerator> Generators;

    public HotReloader(ILogger logger, ContentStack stack, IVirtualFileSystem fileSystem, IReadOnlyList<IContentGenerator> generators)
    {
        this.Logger = logger.ForContext<HotReloader>();
        this.Stack = stack;
        this.FileSystem = fileSystem;
        this.Generators = generators.ToDictionary(x => x.GeneratorKey);
    }

    public void Register(IContent content)
    {
        foreach (var dependency in content.Dependencies)
        {
            this.FileSystem.WatchFile(dependency);
        }
    }

    public void ReloadChangedContent()
    {
        foreach (var file in this.FileSystem.GetChangedFiles())
        {
            foreach (var content in this.Stack)
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
        }
    }
}
