using Mini.Engine.Content.Textures;
using Mini.Engine.Content.v2.Serialization;
using SuperCompressed;

namespace Mini.Engine.Content.v2.Textures;
public static class SdrTextureWriter
{
    public static void Write(ContentWriter contentWriter, Image image, TextureLoaderSettings settings)
    {
        var encoded = Encoder.Instance.Encode(image, settings.Mode, MipMapGeneration.Lanczos3, Quality.Default);                
        contentWriter.Writer.Write(image.Width);
        contentWriter.Writer.Write(image.Height);
        contentWriter.WriteArray(encoded);
    }
}
