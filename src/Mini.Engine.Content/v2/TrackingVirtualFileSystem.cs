using Mini.Engine.IO;

namespace Mini.Engine.Content.v2;
internal class TrackingVirtualFileSystem : IVirtualFileSystem
{
    private readonly IVirtualFileSystem VirtualFileSystem;
    private readonly List<string> Dependencies;

    public TrackingVirtualFileSystem(IVirtualFileSystem virtualFileSystem)
    {
        this.VirtualFileSystem = virtualFileSystem;
        this.Dependencies = new List<string>();
    }

    public IReadOnlyList<string> GetDependencies()
    {
        return this.Dependencies;
    }

    public bool Exists(string path)
    {
        return this.VirtualFileSystem.Exists(path);
    }    

    public Stream OpenRead(string path)
    {
        this.Dependencies.Add(path);
        return this.VirtualFileSystem.OpenRead(path);
    }

    public byte[] ReadAllBytes(string path)
    {
        this.Dependencies.Add(path);
        return this.VirtualFileSystem.ReadAllBytes(path);
    }

    public string ReadAllText(string path)
    {
        this.Dependencies.Add(path);
        return this.VirtualFileSystem.ReadAllText(path);
    }

    public void Create(string path, ReadOnlySpan<byte> contents)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<string> GetChangedFiles()
    {
        throw new NotImplementedException();
    }

    public DateTime GetLastWriteTime(string path)
    {
        throw new NotImplementedException();
    }

    public string NormalizePath(string path)
    {
        throw new NotImplementedException();
    }

    public void WatchFile(string path)
    {
        throw new NotImplementedException();
    }
}
