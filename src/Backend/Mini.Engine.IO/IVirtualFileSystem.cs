namespace Mini.Engine.IO;


public interface IReadOnlyVirtualFileSystem
{
    Stream OpenRead(string path);
    
    string ReadAllText(string path);
    byte[] ReadAllBytes(string path);
    string NormalizePath(string path);
    bool Exists(string path);
    DateTime GetLastWriteTime(string path);
}

public interface IDevelopmentFileSystem
{
    IEnumerable<string> GetChangedFiles();
    void WatchFile(string path);
}

public interface IVirtualFileSystem : IReadOnlyVirtualFileSystem, IDevelopmentFileSystem
{
    Stream CreateWriteRead(string path);
    void Create(string path, ReadOnlySpan<byte> contents);           
}
