using Mini.Engine.Content.Textures;
using Mini.Engine.Content.v2.Serialization;
using StbImageSharp;

namespace Mini.Engine.Content.v2.Textures.Writers;
public static class HdrTextureWriter
{
    public static void Write(ContentWriter contentWriter, Guid header, int version, TextureLoaderSettings settings, ISet<string> dependencies, ImageResultFloat image)
    {
        var floats = image.Data;
        unsafe
        {
            fixed (float* ptr = floats)
            {
                var data = new ReadOnlySpan<byte>(ptr, image.Data.Length * 4);
                contentWriter.WriteHeader(header, version, dependencies);
                contentWriter.Write(settings);
                contentWriter.Writer.Write(CountComponents(image.Comp));
                contentWriter.Writer.Write(image.Width);
                contentWriter.Writer.Write(image.Height);
                contentWriter.WriteArray(data);
            }
        }
    }

    private static int CountComponents(ColorComponents components)
    {
        return components switch
        {
            ColorComponents.Grey => 1,
            ColorComponents.GreyAlpha => 2,
            ColorComponents.RedGreenBlue => 3,
            ColorComponents.RedGreenBlueAlpha => 4,
            _ => throw new ArgumentOutOfRangeException(nameof(components)),
        };
    }
}
