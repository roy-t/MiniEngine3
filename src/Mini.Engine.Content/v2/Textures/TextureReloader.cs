using Mini.Engine.Content.v2.Serialization;

namespace Mini.Engine.Content.v2.Textures;
public static class TextureReloader
{
    public static void Reload(IContentGenerator<TextureContent> generator, TextureContent original, TrackingVirtualFileSystem fileSystem, Stream rwStream)
    {        
        using var writer = new ContentWriter(rwStream);
        generator.Generate(original.Id, original.Meta, fileSystem, writer);

        rwStream.Seek(0, SeekOrigin.Begin);

        using var reader = new ContentReader(rwStream);
        var texture = generator.Load(original.Id, reader);

        original.Reload(texture);
    }
}
