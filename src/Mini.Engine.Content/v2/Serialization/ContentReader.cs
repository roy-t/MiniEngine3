using Mini.Engine.Content.Materials;
using Mini.Engine.Content.Models;
using Mini.Engine.Content.Textures;
using Mini.Engine.IO;
using SuperCompressed;

namespace Mini.Engine.Content.v2.Serialization;
internal sealed class ContentReader : IDisposable
{
    private readonly Stream Stream;
    private readonly BinaryReader Reader;

    internal ContentReader(ContentId id, IVirtualFileSystem fileSystem)
    {
        var path = id.Path + Constants.Extension;

        this.Stream = fileSystem.OpenRead(path);
        this.Reader = new BinaryReader(this.Stream);
    }

    public ContentBlob ReadAll(Guid header)
    {
        var guid = this.ReadHeader(header);        
        if (guid != header)
        {
            throw new Exception($"Header mismatch, expected: {header}, actual: {guid}");
        }

        var meta = this.ReadMeta();
        var dependencies = this.ReadDependencies();
        var contents = this.ReadContents();

        return new ContentBlob(guid, meta, dependencies, contents);
    }
    public void Dispose()
    {
        this.Reader.Dispose();
        this.Stream.Dispose();
    }

    private Guid ReadHeader(Guid guid)
    {
        var buffer = new byte[16];
        this.Reader.Read(buffer);

        return new Guid(buffer);
    }

    private ContentRecord ReadMeta()
    {
        var textureSettings = this.ReadTextureSettings();
        var materialSettings = this.ReadMaterialSettings();
        var modelSettings = this.ReadModelSettings();

        return new ContentRecord(textureSettings, materialSettings, modelSettings);
    }

    private IReadOnlyList<string> ReadDependencies()
    {
        var dependencies = this.Reader.ReadString();
        return dependencies.Split(Constants.StringSeperator);
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
}
