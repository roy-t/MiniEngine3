using System.Text;
using Mini.Engine.Content.Materials;
using Mini.Engine.Content.Models;
using Mini.Engine.Content.Textures;
using SuperCompressed;

namespace Mini.Engine.Content.v2.Serialization;
internal static class ContentReader
{
    public static ContentBlob ReadAll(Stream stream)
    {
        using var reader = new BinaryReader(stream, Encoding.UTF8, true);

        var (guid, timestamp) = ReadHeader(reader);
        var meta = ReadMeta(reader);
        var dependencies = ReadDependencies(reader);
        var contents = ReadContents(reader);

        return new ContentBlob(guid, timestamp, meta, dependencies, contents);
    }

    private static (Guid, DateTime) ReadHeader(BinaryReader reader)
    {
        var buffer = new byte[16];
        reader.Read(buffer);
        var guid = new Guid(buffer);

        var ticks = reader.ReadInt64();
        var timestamp = new DateTime(ticks);

        return (guid, timestamp);
    }

    private static ContentRecord ReadMeta(BinaryReader reader)
    {
        var textureSettings = ReadTextureSettings(reader);
        var materialSettings = ReadMaterialSettings(reader);
        var modelSettings = ReadModelSettings(reader);

        return new ContentRecord(textureSettings, materialSettings, modelSettings);
    }

    private static ISet<string> ReadDependencies(BinaryReader reader)
    {
        var dependencies = reader.ReadString();
        return new HashSet<string>(dependencies.Split(Constants.StringSeperator), new PathComparer());
    }

    private static byte[] ReadContents(BinaryReader reader)
    {
        var length = reader.ReadInt32();
        var bytes = new byte[length];

        reader.Read(bytes);

        return bytes;
    }

    private static TextureLoaderSettings ReadTextureSettings(BinaryReader reader)
    {
        var mode = (Mode)reader.ReadInt32();
        var shouldMipMap = reader.ReadBoolean();

        return new TextureLoaderSettings(mode, shouldMipMap);
    }

    private static MaterialLoaderSettings ReadMaterialSettings(BinaryReader reader)
    {
        var albedo = ReadTextureSettings(reader);
        var metalicness = ReadTextureSettings(reader);
        var normal = ReadTextureSettings(reader);
        var roughness = ReadTextureSettings(reader);
        var ambientOcclusion = ReadTextureSettings(reader);

        return new MaterialLoaderSettings(albedo, metalicness, normal, roughness, ambientOcclusion);
    }

    private static ModelLoaderSettings ReadModelSettings(BinaryReader reader)
    {
        var material = ReadMaterialSettings(reader);
        return new ModelLoaderSettings(material);
    }
}
