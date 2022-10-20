using System.Text;
using Mini.Engine.Content.Materials;
using Mini.Engine.Content.Models;
using Mini.Engine.Content.Textures;

namespace Mini.Engine.Content.v2.Serialization;
internal static class ContentWriter
{
    public static void WriteAll(Stream stream, Guid header, ContentRecord meta, IReadOnlyList<string> dependencies, ReadOnlySpan<byte> contents)
    {
        using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
        WriteHeader(writer, header);
        WriteMeta(writer, meta);
        WriteDependencies(writer, dependencies);
        WriteContents(writer, contents);        
    }

    private static void WriteHeader(BinaryWriter writer, Guid header)
    {
        writer.Write(header.ToByteArray());
        writer.Write(DateTime.Now.Ticks);
    }

    private static void WriteMeta(BinaryWriter writer, ContentRecord record)
    {
        Write(writer, record.TextureSettings);
        Write(writer, record.MaterialSettings);
        Write(writer, record.ModelSettings);
    }

    private static void WriteDependencies(BinaryWriter writer, IReadOnlyList<string> dependencies)
    {
        writer.Write(string.Join(Constants.StringSeperator, dependencies));
    }

    private static void WriteContents(BinaryWriter writer, ReadOnlySpan<byte> bytes)
    {
        writer.Write(bytes.Length);
        writer.Write(bytes);
    }

    private static void Write(BinaryWriter writer, TextureLoaderSettings textureSettings)
    {
        writer.Write((int)textureSettings.Mode);
        writer.Write(textureSettings.ShouldMipMap);
    }

    private static void Write(BinaryWriter writer, MaterialLoaderSettings materialSettings)
    {
        Write(writer, materialSettings.AlbedoFormat);
        Write(writer, materialSettings.MetalicnessFormat);
        Write(writer, materialSettings.NormalFormat);
        Write(writer, materialSettings.RoughnessFormat);
        Write(writer, materialSettings.AmbientOcclusionFormat);
    }

    private static void Write(BinaryWriter writer, ModelLoaderSettings modelSettings)
    {
        Write(writer, modelSettings.MaterialSettings);
    }
}
