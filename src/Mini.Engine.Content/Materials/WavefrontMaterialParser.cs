using Mini.Engine.Content.Materials.Wavefront;
using Mini.Engine.IO;

namespace Mini.Engine.Content.Materials;

internal sealed class WavefrontMaterialParser
{
    private static readonly ContentId Albedo = new(@"albedo.tga");
    private static readonly ContentId Metalicness = new(@"metalicness.tga");
    private static readonly ContentId Normal = new(@"normal.tga");
    private static readonly ContentId Roughness = new(@"roughness.tga");
    private static readonly ContentId AmbientOcclusion = new(@"ao.tga");

    private readonly MtlStatementParser[] Parsers;

    public WavefrontMaterialParser()
    {
        this.Parsers = new MtlStatementParser[]
        {
            new NewMtlParser(),
            new AlbedoParser(),
            new AmbientOcclusionParser(),
            new MetalicnessParser(),
            new NormalParser(),
            new RoughnessParser()
        };
    }

    public MaterialOffline Parse(ContentId id, IReadOnlyVirtualFileSystem fileSystem)
    {
        var state = new ParseState();
        var text = fileSystem.ReadAllText(id.Path).AsSpan();
        foreach (var line in text.EnumerateLines())
        {
            foreach (var parser in this.Parsers)
            {
                if (parser.Parse(state, line, fileSystem))
                {
                    break;
                }
            }
        }

        state.EndMaterial();

        var record = GetRecord(id, state);

        return new MaterialOffline
        (
            record.Key,
            GetIdOrFallback(id, MaterialType.Albedo, record.Albedo),
            GetIdOrFallback(id, MaterialType.Metalicness, record.Metalicness),
            GetIdOrFallback(id, MaterialType.Normal, record.Normal),
            GetIdOrFallback(id, MaterialType.Roughness, record.Roughness),
            GetIdOrFallback(id, MaterialType.AmbientOcclusion, record.AmbientOcclusion)
        );
    }

    private static MaterialRecords GetRecord(ContentId id, ParseState state)
    {
        var match = state.Materials.Find(m => id.Key.Equals(m.Key, StringComparison.InvariantCultureIgnoreCase))
            ?? throw new KeyNotFoundException($"Could not find material {id.Key} in material library {id.Path}");

        return match;
    }

    private static ContentId GetIdOrFallback(ContentId parent, MaterialType type, string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return type switch
            {
                MaterialType.Albedo => Albedo,
                MaterialType.Metalicness => Metalicness,
                MaterialType.Normal => Normal,
                MaterialType.Roughness => Roughness,
                MaterialType.AmbientOcclusion => AmbientOcclusion,
                _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unsupported material type {type}"),
            };
        }

        return parent.RelativeTo(path);
    }
}
