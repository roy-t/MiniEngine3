using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
//using Mini.Engine.Content.Models.Wavefront;
//using Mini.Engine.Content.Textures;
//using Mini.Engine.IO;
//using Serilog.Core;

BenchmarkRunner.Run<Bench>();

public class Bench
{
    //private readonly ModelLoader Loader = new(Logger.None, new TextureLoader(Logger.None));
    //private readonly IVirtualFileSystem FileSystem = new DiskFileSystem(Logger.None, @"D:\Projects\C#\MiniEngine3\src\Mini.Engine.Content\Models\sponza");

    [Benchmark]
    public void Slices()
    {
        //this.Loader.Load(this.FileSystem, "sponza.obj");
    }
}
