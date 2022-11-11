using Mini.Engine.Content.Serialization;
using SuperCompressed;

namespace Mini.Engine.Content.Textures;
public static class SdrTextureWriter
{
    public static void Write(ContentWriter contentWriter, Image image, TextureSettings settings)
    {
        var encoded = Encoder.Instance.Encode(image, settings.Mode, MipMapGeneration.Lanczos3, Quality.Default);
        contentWriter.Writer.Write(image.Width);
        contentWriter.Writer.Write(image.Height);
        contentWriter.WriteArray(encoded);
    }
}
