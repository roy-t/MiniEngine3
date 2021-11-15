using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Mini.Engine.Content.Models.Obj;
using Mini.Engine.IO;
using Serilog.Core;

var summary = BenchmarkRunner.Run<Bench>();

public class Bench
{
    private readonly ObjModelLoader Loader = new(Logger.None);
    private readonly IVirtualFileSystem FileSystem = new DiskFileSystem(Logger.None, @"D:\Projects\C#\MiniEngine3\src\Mini.Engine.Content\Models\sponza");

    [Benchmark]
    public void Slices()
    {
        this.Loader.Load(this.FileSystem, "sponza.obj");
    }
}




