using Mini.Engine.Content.Serialization;
using Mini.Engine.DirectX.Resources.Models;
using Mini.Engine.IO;

namespace Mini.Engine.Content.Materials;
internal sealed class WavefrontMaterialProcessor : ManagedContentProcessor<IMaterial, MaterialContent, MaterialSettings>
{
    private const int ProcessorVersion = 1;
    private static readonly Guid ProcessorType = new("{0124D18A-D3E6-48C4-A733-BD3881171B76}");

    private readonly WavefrontMaterialParser Parser;
    private readonly ContentManager Content;

    public WavefrontMaterialProcessor(ContentManager content)
        : base(ProcessorVersion, ProcessorType, ".mtl")
    {
        this.Parser = new WavefrontMaterialParser();
        this.Content = content;
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
        var albedo = this.Content.LoadTexture(reader.ReadContentId(), settings.AlbedoFormat);
        var metalicness = this.Content.LoadTexture(reader.ReadContentId(), settings.MetalicnessFormat);
        var normal = this.Content.LoadTexture(reader.ReadContentId(), settings.NormalFormat);
        var roughness = this.Content.LoadTexture(reader.ReadContentId(), settings.RoughnessFormat);
        var ambientOcclusion = this.Content.LoadTexture(reader.ReadContentId(), settings.AmbientOcclusionFormat);

        return new Material(name, albedo, metalicness, normal, roughness, ambientOcclusion);
    }

    public override MaterialContent Wrap(ContentId id, IMaterial content, MaterialSettings settings, ISet<string> dependencies)
    {
        return new MaterialContent(id, content, settings, dependencies);
    }
}
