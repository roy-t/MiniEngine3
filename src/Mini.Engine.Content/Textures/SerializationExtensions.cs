using Mini.Engine.Content.Serialization;
using SuperCompressed;

namespace Mini.Engine.Content.Textures;
public static class SerializationExtensions
{
    public static void Write(this ContentWriter writer, TextureSettings settings)
    {
        writer.Write((int)settings.Mode);
        writer.Write(settings.ShouldMipMap);
    }

    public static TextureSettings ReadTextureSettings(this ContentReader reader)
    {
        var mode = (Mode)reader.ReadInt();
        var shouldMipMap = reader.ReadBool();

        return new TextureSettings(mode, shouldMipMap);
    }
}
