using Mini.Engine.IO;
using Serilog;
using SuperCompressed;

namespace Mini.Engine.Content.Textures;

internal sealed class TextureCompressor
{
    private readonly static string ExpectedExtension = ".uastc";

    private readonly static string[] SupportedExtensions = new[]
    {
        ".jpg", ".jpeg", ".png", ".bmp", ".tga", ".psd", ".gif"
    };

    private readonly ILogger Logger;
    private readonly IVirtualFileSystem FileSystem;
    private readonly Dictionary<string, (ContentId, TextureLoaderSettings)> Settings;

    public TextureCompressor(ILogger logger, IVirtualFileSystem fileSystem)
    {
        this.Logger = logger.ForContext<TextureCompressor>();
        this.FileSystem = fileSystem;
        this.Settings = new Dictionary<string, (ContentId, TextureLoaderSettings)>();
    }

    internal void Register(ContentId id, TextureLoaderSettings settings)
    {
        var sourceFile = this.FindSourceFile(id);
        if (sourceFile != null)
        {
            this.FileSystem.WatchFile(sourceFile);
            this.Settings.Add(sourceFile, (id, settings));
        }
    }

    internal void CompressSourceFileFor(ContentId id, TextureLoaderSettings settings)
    {        
        var sourceFile = this.FindSourceFile(id);
        if (sourceFile == null)
        {
            var all = "{" + string.Join(", ", SupportedExtensions) + "}";
            throw new FileNotFoundException($"Could not find file {Path.GetFileNameWithoutExtension(id.Path)}{all}");
        }

        this.Logger.Information($"Compressing texture file {sourceFile}->{id.Path} in the foreground");
        Compress(sourceFile, id, settings);
    }

    internal void ProcessChangedFile(string file)
    {
        if (this.Settings.TryGetValue(file, out var tuple))
        {
            var id = tuple.Item1;
            var settings = tuple.Item2;

            this.Logger.Information($"Compressing texture file {file}->{id.Path} in the background");
            Compress(file, id, settings);
        }
    }

    private string? FindSourceFile(ContentId id)
    {
        if (id.Path.EndsWith(ExpectedExtension))
        {
            var basePath = id.Path[..^ExpectedExtension.Length];
            foreach (var extension in SupportedExtensions)
            {
                var fullPath = basePath + extension;
                if (this.FileSystem.Exists(fullPath))
                {
                    return fullPath;
                }
            }
        }

        return null;
    }

    public void Compress(string file, ContentId id, TextureLoaderSettings settings)
    {
        var bytes = this.FileSystem.ReadAllBytes(file);
        var image = Image.FromMemory(bytes);

        var encoded = Encoder.Instance.Encode(image, settings.Mode, MipMapGeneration.Lanczos3, Quality.Default);

        this.FileSystem.Create(id.Path, encoded);
    }
}
