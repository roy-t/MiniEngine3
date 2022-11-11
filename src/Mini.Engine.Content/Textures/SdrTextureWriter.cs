using Mini.Engine.Content.Serialization;
using SuperCompressed;

namespace Mini.Engine.Content.Textures;
public static class SdrTextureWriter
{
    public static void Write(ContentWriter contentWriter, Image image, TextureSettings settings)
    {
        var encoded = Encoder.Instance.Encode(image, settings.Mode, MipMapGeneration.Lanczos3, Quality.Default);
        contentWriter.Write(image.Width);
        contentWriter.Write(image.Height);
        contentWriter.Write(encoded);
    }
}
