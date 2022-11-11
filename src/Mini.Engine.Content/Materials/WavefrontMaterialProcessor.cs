using Mini.Engine.Content.Serialization;
using Mini.Engine.Content.Textures;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Models;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.IO;

namespace Mini.Engine.Content.Materials;
internal sealed class WavefrontMaterialProcessor : UnmanagedContentProcessor<IMaterial, MaterialContent, MaterialSettings>
{
    private const int ProcessorVersion = 1;
    private static readonly Guid ProcessorType = new("{0124D18A-D3E6-48C4-A733-BD3881171B76}");

    private readonly Device Device;
    private readonly ContentManager Content;
    private readonly WavefrontMaterialParser Parser;

    public WavefrontMaterialProcessor(Device device, ContentManager content)
        : base(device.Resources, ProcessorVersion, ProcessorType, ".mtl")
    {
        this.Device = device;
        this.Content = content;

        this.Parser = new WavefrontMaterialParser();
    }

    protected override void WriteSettings(ContentId id, MaterialSettings settings, ContentWriter writer)
    {
        writer.Write(settings);
    }

    protected override void WriteBody(ContentId id, MaterialSettings settings, ContentWriter writer, IReadOnlyVirtualFileSystem fileSystem)
    {
        var material = this.Parser.Parse(id, fileSystem);

        writer.Writer.Write(material.Name);
        writer.Write(material.Albedo);
        writer.Write(material.Metalicness);
        writer.Write(material.Normal);
        writer.Write(material.Roughness);
        writer.Write(material.AmbientOcclusion);
    }

    protected override MaterialSettings ReadSettings(ContentId id, ContentReader reader)
    {
        return reader.ReadMaterialSettings();
    }

    protected override IMaterial ReadBody(ContentId id, MaterialSettings settings, ContentReader reader)
    {
        var name = reader.Reader.ReadString();

        var albedo = this.LoadTexture(reader.ReadContentId(), settings.AlbedoFormat);
        var metalicness = this.LoadTexture(reader.ReadContentId(), settings.MetalicnessFormat);
        var normal = this.LoadTexture(reader.ReadContentId(), settings.NormalFormat);
        var roughness = this.LoadTexture(reader.ReadContentId(), settings.RoughnessFormat);
        var ambientOcclusion = this.LoadTexture(reader.ReadContentId(), settings.AmbientOcclusionFormat);

        return new Material(name, albedo, metalicness, normal, roughness, ambientOcclusion);
    }

    public override MaterialContent Wrap(ContentId id, IMaterial content, MaterialSettings settings, ISet<string> dependencies)
    {
        return new MaterialContent(id, content, settings, dependencies);
    }

    private ITexture LoadTexture(ContentId id, TextureSettings settings)
    {
        var reference = this.Content.LoadTexture(id, settings);
        return this.Device.Resources.Get(reference);
    }
}
