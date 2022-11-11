using System.Text;
using Mini.Engine.Content.Materials;
using Mini.Engine.Content.Models;
using Mini.Engine.Content.Textures;
using SuperCompressed;

namespace Mini.Engine.Content.Serialization;

public sealed class ContentReader : IDisposable
{
    public ContentReader(Stream stream)
    {
        this.Reader = new BinaryReader(stream, Encoding.UTF8, true);
    }

    public BinaryReader Reader { get; }

    public ContentHeader ReadHeader()
    {
        var (guid, timestamp) = this.ReadType();
        var version = this.Reader.ReadInt32();
        var dependencies = this.ReadDependencies();

        return new ContentHeader(guid, version, timestamp, dependencies);
    }

    public ContentId ReadContentId()
    {
        var path = this.Reader.ReadString();
        var key = this.Reader.ReadString();

        return new ContentId(path, key);
    }

    public byte[] ReadArray()
    {
        var length = this.Reader.ReadInt32();
        var bytes = new byte[length];

        this.Reader.Read(bytes);

        return bytes;
    }

    private (Guid, DateTime) ReadType()
    {
        var buffer = new byte[16];
        this.Reader.Read(buffer);
        var guid = new Guid(buffer);

        var ticks = this.Reader.ReadInt64();
        var timestamp = new DateTime(ticks);

        return (guid, timestamp);
    }

    public TextureSettings ReadTextureSettings()
    {
        var mode = (Mode)this.Reader.ReadInt32();
        var shouldMipMap = this.Reader.ReadBoolean();

        return new TextureSettings(mode, shouldMipMap);
    }

    public MaterialSettings ReadMaterialSettings()
    {
        var albedo = this.ReadTextureSettings();
        var metalicness = this.ReadTextureSettings();
        var normal = this.ReadTextureSettings();
        var roughness = this.ReadTextureSettings();
        var ambientOcclusion = this.ReadTextureSettings();

        return new MaterialSettings(albedo, metalicness, normal, roughness, ambientOcclusion);
    }

    public ModelSettings ReadModelSettings()
    {
        var material = this.ReadMaterialSettings();
        return new ModelSettings(material);
    }

    public void Dispose()
    {
        this.Reader.Dispose();
    }

    private ISet<string> ReadDependencies()
    {
        var dependencies = this.Reader.ReadString();
        if (string.IsNullOrEmpty(dependencies))
        {
            return new HashSet<string>(0);
        }
        return new HashSet<string>(dependencies.Split(ContentWriter.DependencySeperator), new PathComparer());
    }
}
