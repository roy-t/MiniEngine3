﻿using Mini.Engine.IO;

namespace Mini.Engine.Content.Serialization;
public sealed class TrackingVirtualFileSystem : IReadOnlyVirtualFileSystem
{
    private readonly IReadOnlyVirtualFileSystem VirtualFileSystem;
    private readonly HashSet<string> Dependencies;

    public TrackingVirtualFileSystem(IReadOnlyVirtualFileSystem virtualFileSystem)
    {
        this.VirtualFileSystem = virtualFileSystem;
        this.Dependencies = new HashSet<string>(new PathComparer());
    }

    public ISet<string> GetDependencies()
    {
        return this.Dependencies;
    }

    public void AddDependency(string path)
    {
        this.Dependencies.Add(path);
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

    public string NormalizePath(string path)
    {
        return this.VirtualFileSystem.NormalizePath(path);
    }

    public DateTime GetLastWriteTime(string path)
    {
        return this.VirtualFileSystem.GetLastWriteTime(path);
    }
}
