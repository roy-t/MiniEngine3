using System;
using System.Collections.Generic;
using System.IO;
using Serilog;

namespace Mini.Engine.IO;

public sealed class DiskFileSystem : IVirtualFileSystem
{
    private readonly ILogger Logger;
    private readonly FileSystemWatcher FileSystemWatcher;

    private readonly HashSet<string> ChangedFilesFilter;
    private readonly HashSet<string> ChangedFiles;

    public DiskFileSystem(ILogger logger, string rootDirectory)
    {
        this.Logger = logger.ForContext<DiskFileSystem>();
        this.RootDirectory = Path.GetFullPath(rootDirectory);

        this.FileSystemWatcher = CreateWatcher(this.RootDirectory);
        this.FileSystemWatcher.Changed += (s, e) => this.OnChange(e.FullPath, "Changed");

        this.ChangedFilesFilter = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        this.ChangedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    public string RootDirectory { get; }

    public Stream OpenRead(string path)
    {
        return File.OpenRead(this.ToAbsolute(path));
    }

    public string ReadAllText(string path)
    {
        return File.ReadAllText(this.ToAbsolute(path));
    }

    public string[] ReadAllLines(string path)
    {
        return File.ReadAllLines(this.ToAbsolute(path));
    }

    public void WatchFile(string path)
    {
        this.ChangedFilesFilter.Add(path);
    }

    public IEnumerable<string> GetChangedFiles()
    {
        return this.ChangedFiles;
    }

    public void ClearChangedFiles()
    {
        this.ChangedFiles.Clear();
    }

    private string ToAbsolute(string path)
    {
        if (Path.IsPathRooted(path))
        {
            throw new ArgumentException($"Expected relative path but got '{path}'", nameof(path));
        }

        return Path.Combine(this.RootDirectory, path);
    }

    private string ToRelative(string path)
    {
        if (path.StartsWith(this.RootDirectory, StringComparison.OrdinalIgnoreCase))
        {
            return path.Substring(this.RootDirectory.Length + 1);
        }

        throw new ArgumentException($"Expected absolute path but got '{path}'", nameof(path));
    }

    private void OnChange(string fullPath, string reason)
    {
        var relativePath = ToRelative(fullPath);
        this.Logger.Debug("[{@reason}] {@file}", reason, relativePath);

        if (this.ChangedFilesFilter.Contains(relativePath))
        {
            this.ChangedFiles.Add(relativePath);
        }
    }

    private static FileSystemWatcher CreateWatcher(string directory)
    {
        return new FileSystemWatcher(directory)
        {
            NotifyFilter = NotifyFilters.Attributes
                        | NotifyFilters.CreationTime
                        | NotifyFilters.DirectoryName
                        | NotifyFilters.FileName
                        | NotifyFilters.LastAccess
                        | NotifyFilters.LastWrite
                        | NotifyFilters.Security
                        | NotifyFilters.Size,
            Filter = "*",
            IncludeSubdirectories = true,
            EnableRaisingEvents = true,
        };
    }
}
