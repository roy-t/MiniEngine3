﻿namespace Mini.Engine.IO;

public interface IVirtualFileSystem
{
    Stream OpenRead(string path);
    string ReadAllText(string path);
    string NormalizePath(string path);
    IEnumerable<string> GetChangedFiles();
    void WatchFile(string path);
    byte[] ReadAllBytes(string path);
}
