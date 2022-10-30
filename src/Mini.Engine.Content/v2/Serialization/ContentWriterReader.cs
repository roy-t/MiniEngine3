namespace Mini.Engine.Content.v2.Serialization;
public sealed class ContentWriterReader : IDisposable
{
    private readonly long StartPosition;
    private readonly Stream Stream;

    public ContentWriterReader(Stream stream)
    {
        this.Writer = new ContentWriter(stream);
        this.Reader = new ContentReader(stream);

        this.StartPosition = stream.Position;
        this.Stream = stream;
    }

    public ContentWriter Writer { get; }
    public ContentReader Reader { get; }    

    public void Rewind()
    {
        this.Stream.Position = this.StartPosition;
    }

    public void Dispose()
    {
        this.Writer.Dispose();
        this.Reader.Dispose();
    }
}
