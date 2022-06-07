using Serilog;

namespace Mini.Engine.IO;

public sealed class DiskFileSystem : IVirtualFileSystem
{
    private readonly ILogger Logger;
    private readonly FileSystemWatcher FileSystemWatcher;

    private readonly HashSet<string> ChangedFilesFilter;
    private readonly DelayedSet<string> ChangedFiles;

    public DiskFileSystem(ILogger logger, string rootDirectory)
    {
        this.Logger = logger.ForContext<DiskFileSystem>();
        this.RootDirectory = Path.GetFullPath(rootDirectory);

        this.FileSystemWatcher = CreateWatcher(this.RootDirectory);
        this.FileSystemWatcher.Changed += (s, e) => this.OnChange(e.FullPath, e.ChangeType.ToString());
        this.FileSystemWatcher.Renamed += (s, e) => this.OnChange(e.FullPath, e.ChangeType.ToString());

        this.ChangedFilesFilter = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        this.ChangedFiles = new DelayedSet<string>(TimeSpan.FromSeconds(1), StringComparer.OrdinalIgnoreCase);
    }

    public string RootDirectory { get; }

    public Stream OpenRead(string path)
    {
        return File.Open(this.ToAbsolute(path), FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
    }

    public string NormalizePath(string relativePath)
    {
        return this.ToRelative(this.ToAbsolute(relativePath)).ToLowerInvariant();
    }

    public string ReadAllText(string path)
    {
        using var stream = this.OpenRead(path);
        return new StreamReader(stream).ReadToEnd();
    }

    public byte[] ReadAllBytes(string path)
    {
        return File.ReadAllBytes(path);
    }

    public void WatchFile(string path)
    {
        var normalized = this.NormalizePath(path);
        this.ChangedFilesFilter.Add(normalized);
    }

    public IEnumerable<string> GetChangedFiles()
    {
        return this.ChangedFiles.PopAvailable();
    }

    public string ToAbsolute(string path)
    {
        if (Path.IsPathRooted(path))
        {
            throw new ArgumentException($"Expected relative path but got '{path}'", nameof(path));
        }

        return Path.GetFullPath(path, this.RootDirectory);
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
        var relativePath = this.ToRelative(fullPath);
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
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
            Filter = "*",
            IncludeSubdirectories = true,
            EnableRaisingEvents = true,
        };
    }

    /// <summary>
    /// Makes items in the set available a fixed delay after adding them
    /// </summary>
    private sealed class DelayedSet<T>
        where T : notnull
    {
        private readonly TimeSpan Delay;
        private readonly Dictionary<T, DateTime> Entries;

        public DelayedSet(TimeSpan delay, IEqualityComparer<T> comparer)
        {
            this.Delay = delay;
            this.Entries = new Dictionary<T, DateTime>(comparer);
        }

        public void Add(T data)
        {
            if (!this.Entries.ContainsKey(data))
            {
                this.Entries.Add(data, DateTime.Now + this.Delay);
            }
        }

        public IEnumerable<T> PopAvailable()
        {
            var popped = new List<T>();
            var cutoff = DateTime.Now;
            foreach (var entry in this.Entries)
            {
                if (entry.Value < cutoff)
                {
                    popped.Add(entry.Key);
                    yield return entry.Key;
                }
            }

            foreach (var pop in popped)
            {
                this.Entries.Remove(pop);
            }
        }
    }
}
