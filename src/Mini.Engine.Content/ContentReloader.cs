using Mini.Engine.Content.Serialization;

namespace Mini.Engine.Content;

public static class ContentReloader
{
    public static void Reload<TContent, TWrapped, TSettings>(IContentProcessor<TContent, TWrapped, TSettings> generator, TWrapped original, TrackingVirtualFileSystem fileSystem, ContentWriterReader writerReader)
        where TContent : IDisposable
        where TWrapped : IContent<TContent, TSettings>, TContent
    {
        generator.Generate(original.Id, original.Settings, writerReader.Writer, fileSystem);

        writerReader.Rewind();

        var header = writerReader.Reader.ReadHeader();
        var texture = generator.Load(original.Id, header, writerReader.Reader);
        original.Reload(texture);
    }
}
