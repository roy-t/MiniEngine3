using System.Text;

namespace Mini.Engine.Content.Serialization;

public sealed class ContentReader : IDisposable
{
    public ContentReader(Stream stream)
    {
        this.Reader = new BinaryReader(stream, Encoding.UTF8, true);
    }

    public BinaryReader Reader { get; }

    public ContentHeader ReadHeader()
    {
        var (guid, timestamp) = this.ReadType();
        var version = this.Reader.ReadInt32();
        var dependencies = this.ReadDependencies();

        return new ContentHeader(guid, version, timestamp, dependencies);
    }

    public bool ReadBool()
    {
        return this.Reader.ReadBoolean();
    }

    public int ReadInt()
    {
        return this.Reader.ReadInt32();
    }

    public float ReadFloat()
    {
        return this.Reader.ReadSingle();
    }

    public string ReadString()
    {
        return this.Reader.ReadString();
    }

    public byte[] ReadBytes()
    {
        var length = this.Reader.ReadInt32();
        var bytes = new byte[length];

        this.Reader.Read(bytes);

        return bytes;
    }

    public void Dispose()
    {
        this.Reader.Dispose();
    }

    private (Guid, DateTime) ReadType()
    {
        var buffer = new byte[16];
        this.Reader.Read(buffer);
        var guid = new Guid(buffer);

        var ticks = this.Reader.ReadInt64();
        var timestamp = new DateTime(ticks);

        return (guid, timestamp);
    }

    private ISet<string> ReadDependencies()
    {
        var dependencies = this.Reader.ReadString();
        if (string.IsNullOrEmpty(dependencies))
        {
            return new HashSet<string>(0);
        }
        return new HashSet<string>(dependencies.Split(ContentWriter.DependencySeperator), new PathComparer());
    }
}
