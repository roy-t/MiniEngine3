using Mini.Engine.Content.Textures;
using Mini.Engine.Content.v2.Serialization;
using SuperCompressed;

namespace Mini.Engine.Content.v2.Textures.Writers;
public static class CompressedTextureWriter
{
    public static void Write(ContentWriter contentWriter, Guid header, int version, TextureLoaderSettings settings, ISet<string> dependencies, Image image)
    {
        var encoded = Encoder.Instance.Encode(image, settings.Mode, MipMapGeneration.Lanczos3, Quality.Default);
        contentWriter.WriteHeader(header, version, dependencies);
        contentWriter.Write(settings);
        contentWriter.WriteArray(encoded);
    }
}
