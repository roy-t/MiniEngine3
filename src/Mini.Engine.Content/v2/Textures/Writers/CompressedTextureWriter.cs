using Mini.Engine.Content.Textures;
using Mini.Engine.Content.v2.Serialization;
using SuperCompressed;

namespace Mini.Engine.Content.v2.Textures.Writers;
public static class CompressedTextureWriter
{
    public static void Write(ContentWriter contentWriter, int version, TextureLoaderSettings settings, ISet<string> dependencies, Image image)
    {
        var header = (image.Width < TextureConstants.MinBlockSize || image.Height < TextureConstants.MinBlockSize)
           ? TextureConstants.HeaderUncompressed
           : TextureConstants.HeaderCompressed;

        var encoded = Encoder.Instance.Encode(image, settings.Mode, MipMapGeneration.Lanczos3, Quality.Default);
        contentWriter.WriteHeader(header, version, dependencies);
        contentWriter.Write(settings);
        contentWriter.WriteArray(encoded);
    }
}
