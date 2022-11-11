using System.Text;

namespace Mini.Engine.Content.Serialization;
public sealed class ContentWriter : IDisposable
{
    public static char DependencySeperator = ';';

    public ContentWriter(Stream stream)
    {
        this.Writer = new BinaryWriter(stream, Encoding.UTF8, true);
    }

    public BinaryWriter Writer { get; }

    public void WriteHeader(Guid header, int version, ISet<string> dependencies)
    {
        this.WriteType(header);
        this.Writer.Write(version);
        this.WriteDependencies(dependencies);
    }

    public void Write(bool value)
    {
        this.Writer.Write(value);
    }

    public void Write(int value)
    {
        this.Writer.Write(value);
    }

    public void Write(float value)
    {
        this.Writer.Write(value);
    }

    public void Write(string value)
    {
        this.Writer.Write(value);
    }

    public void Write(ReadOnlySpan<byte> bytes)
    {
        this.Writer.Write(bytes.Length);
        this.Writer.Write(bytes);
    }

    public void Dispose()
    {
        this.Writer.Flush();
        this.Writer.Dispose();
    }

    private void WriteType(Guid header)
    {
        this.Writer.Write(header.ToByteArray());
        this.Writer.Write(DateTime.Now.Ticks);
    }

    private void WriteDependencies(ISet<string> dependencies)
    {
        var dependencyString = string.Join(DependencySeperator, dependencies);
        this.Writer.Write(dependencyString);
    }
}
