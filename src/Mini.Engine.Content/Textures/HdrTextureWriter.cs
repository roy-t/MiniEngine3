﻿using Mini.Engine.Content.Serialization;
using StbImageSharp;

namespace Mini.Engine.Content.Textures;
public static class HdrTextureWriter
{
    public static void Write(ContentWriter contentWriter, TextureSettings settings, ImageResultFloat image)
    {
        var floats = image.Data;
        unsafe
        {
            fixed (float* ptr = floats)
            {
                var data = new ReadOnlySpan<byte>(ptr, image.Data.Length * 4);
                contentWriter.Write(CountComponents(image.Comp));
                contentWriter.Write(image.Width);
                contentWriter.Write(image.Height);
                contentWriter.Write(data);
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
