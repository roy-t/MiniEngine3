﻿using Mini.Engine.IO;
using Serilog;
using SuperCompressed;

namespace Mini.Engine.Content.Textures;

internal sealed class TextureCompressor
{
    private readonly static string CompressedExtension = ".uastc";

    private readonly static string[] UncompressexExtensions = new[]
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

    internal void Watch(ContentId id, TextureLoaderSettings settings)
    {
        var sourceFile = this.FindSourceFile(id);
        if (sourceFile != null)
        {
            this.FileSystem.WatchFile(sourceFile);
            this.Settings.Add(sourceFile, (id, settings));
        }
    }
    
    internal void EnsureCompressedFileExistsAndIsUpToDate(ContentId id, TextureLoaderSettings settings)
    {        
        var originalFile = this.FindSourceFile(id);
        if (originalFile != null)
        {
            if (this.FileSystem.Exists(id.Path))
            {
                var originalFileDate = this.FileSystem.GetLastWriteTime(originalFile);
                var compressedFileDate = this.FileSystem.Exists(id.Path)
                    ? this.FileSystem.GetLastWriteTime(id.Path)
                    : DateTime.MinValue;

                if (originalFileDate > compressedFileDate)
                {
                    this.Logger.Information($"Compressing texture file {originalFile} in the foreground because target {id.Path} is outdated");
                    this.Compress(originalFile, id, settings);
                }
            }
            else
            {
                this.Logger.Information($"Compressing texture file {originalFile} in the foreground because target {id.Path} does not exist");
                this.Compress(originalFile, id, settings);
            }
        }
        else
        {
            var all = "{" + string.Join(", ", UncompressexExtensions) + "}";
            throw new FileNotFoundException($"Cannot compress {id.Path}, could not find source file {Path.GetFileNameWithoutExtension(id.Path)}{all} to compress");
        }
    }

    internal void ProcessChangedFile(string file)
    {
        if (this.Settings.TryGetValue(file, out var tuple))
        {
            var id = tuple.Item1;
            var settings = tuple.Item2;

            this.Logger.Information($"Compressing texture file {file}->{id.Path} in the background");
            this.Compress(file, id, settings);
        }
    }

    private string? FindSourceFile(ContentId id)
    {
        if (id.Path.EndsWith(CompressedExtension))
        {
            var basePath = id.Path[..^CompressedExtension.Length];
            foreach (var extension in UncompressexExtensions)
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

    private void Compress(string file, ContentId id, TextureLoaderSettings settings)
    {
        var bytes = this.FileSystem.ReadAllBytes(file);
        var image = Image.FromMemory(bytes);

        var encoded = Encoder.Instance.Encode(image, settings.Mode, MipMapGeneration.Lanczos3, Quality.Default);

        this.FileSystem.Create(id.Path, encoded);
    }
}
