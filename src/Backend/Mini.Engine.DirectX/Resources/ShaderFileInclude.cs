using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Mini.Engine.IO;
using SharpGen.Runtime;
using Vortice.Direct3D;

namespace Mini.Engine.DirectX.Resources;

internal sealed class ShaderFileInclude : CallbackBase, Include
{
    private readonly IVirtualFileSystem FileSystem;
    private readonly string RootFolder;
    private readonly Dictionary<Stream, string> Streams;

    public ShaderFileInclude(IVirtualFileSystem fileSystem, string? rootFolder = null)
    {
        this.FileSystem = fileSystem;
        this.RootFolder = rootFolder ?? Environment.CurrentDirectory;
        this.Streams = new Dictionary<Stream, string>();
    }

    public void Close(Stream stream)
    {
        stream.Close();
    }

    public Stream Open(IncludeType type, string fileName, Stream? parentStream)
    {
        if (parentStream != null)
        {
            var parentFileName = this.Streams[parentStream];
            var folder = Path.GetDirectoryName(parentFileName) ?? string.Empty;
            fileName = Path.Combine(folder, fileName);
        }

        // Ensure C# handles all the text handling, conversions, BOMs, etc...
        // and return the raw ASCII bytes to DirectX
        var path = Path.Combine(this.RootFolder, fileName);
        var text = this.FileSystem.ReadAllText(path);
        var bytes = Encoding.ASCII.GetBytes(text);
        var stream = new MemoryStream(bytes, false);
        this.Streams.Add(stream, fileName);
        return stream;
    }

    protected override void DisposeCore(bool disposing)
    {
        if (disposing)
        {
            foreach(var record in this.Streams)
            {
                record.Key.Dispose();
            }
            this.Streams.Clear();
        }        
    }
}
