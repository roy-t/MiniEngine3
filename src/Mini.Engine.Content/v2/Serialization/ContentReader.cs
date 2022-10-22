using System.Text;
using Mini.Engine.Content.Materials;
using Mini.Engine.Content.Models;
using Mini.Engine.Content.Textures;
using SuperCompressed;

namespace Mini.Engine.Content.v2.Serialization;
public sealed class ContentReader : IDisposable
{
    public ContentReader(Stream stream)
    {
        this.Reader = new BinaryReader(stream, Encoding.UTF8, true);
    }

    public BinaryReader Reader { get; }

    public ContentBlob ReadCommon()
    {
        var (guid, timestamp) = this.ReadHeader();
        var meta = this.ReadMeta();
        var dependencies = this.ReadDependencies();
        var contents = this.ReadContents();

        return new ContentBlob(guid, timestamp, meta, dependencies, contents);
    }

    private (Guid, DateTime) ReadHeader()
    {
        var buffer = new byte[16];
        this.Reader.Read(buffer);
        var guid = new Guid(buffer);

        var ticks = this.Reader.ReadInt64();
        var timestamp = new DateTime(ticks);

        return (guid, timestamp);
    }

    private ContentRecord ReadMeta()
    {
        var textureSettings = this.ReadTextureSettings();
        var materialSettings = this.ReadMaterialSettings();
        var modelSettings = this.ReadModelSettings();

        return new ContentRecord(textureSettings, materialSettings, modelSettings);
    }

    private ISet<string> ReadDependencies()
    {
        var dependencies = this.Reader.ReadString();
        return new HashSet<string>(dependencies.Split(Constants.StringSeperator), new PathComparer());
    }

    private byte[] ReadContents()
    {
        var length = this.Reader.ReadInt32();
        var bytes = new byte[length];

        this.Reader.Read(bytes);

        return bytes;
    }

    private TextureLoaderSettings ReadTextureSettings()
    {
        var mode = (Mode)this.Reader.ReadInt32();
        var shouldMipMap = this.Reader.ReadBoolean();

        return new TextureLoaderSettings(mode, shouldMipMap);
    }

    private MaterialLoaderSettings ReadMaterialSettings()
    {
        var albedo = this.ReadTextureSettings();
        var metalicness = this.ReadTextureSettings();
        var normal = this.ReadTextureSettings();
        var roughness = this.ReadTextureSettings();
        var ambientOcclusion = this.ReadTextureSettings();

        return new MaterialLoaderSettings(albedo, metalicness, normal, roughness, ambientOcclusion);
    }

    private ModelLoaderSettings ReadModelSettings()
    {
        var material = this.ReadMaterialSettings();
        return new ModelLoaderSettings(material);
    }

    public void Dispose()
    {
        this.Reader.Dispose();
    }
}
