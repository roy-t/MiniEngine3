using System.Text;
using Mini.Engine.IO;
using SharpGen.Runtime;
using Vortice.Direct3D;

namespace Mini.Engine.DirectX.Resources.Shaders;

internal sealed class ShaderFileInclude : CallbackBase, Include
{
    private readonly IReadOnlyVirtualFileSystem FileSystem;
    private readonly string RootFolder;
    private readonly Dictionary<Stream, string> Streams;
    private readonly List<string> IncludedList;

    public ShaderFileInclude(IReadOnlyVirtualFileSystem fileSystem, string? rootFolder = null)
    {
        this.FileSystem = fileSystem;
        this.RootFolder = rootFolder ?? Environment.CurrentDirectory;
        this.Streams = new Dictionary<Stream, string>();
        this.IncludedList = new List<string>();
    }

    public IReadOnlyList<string> Included => this.IncludedList;

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
        this.IncludedList.Add(fileName);
        return stream;
    }

    protected override void DisposeCore(bool disposing)
    {
        if (disposing)
        {
            foreach (var record in this.Streams)
            {
                record.Key.Dispose();
            }
            this.Streams.Clear();
        }
    }
}
