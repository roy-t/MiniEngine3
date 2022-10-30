using Mini.Engine.Content.Textures;
using Mini.Engine.Content.v2.Serialization;

namespace Mini.Engine.Content.v2.Textures;
public static class TextureReloader
{
    public static void Reload(IContentProcessor<TextureContent, TextureLoaderSettings> generator, TextureContent original, TrackingVirtualFileSystem fileSystem, ContentWriterReader writerReader)
    {            
        generator.Generate(original.Id, original.Settings, writerReader.Writer, fileSystem);

        writerReader.Rewind();

        var header = writerReader.Reader.ReadHeader();
        var texture = generator.Load(original.Id, header, writerReader.Reader);
        original.Reload(texture);
    }
}
