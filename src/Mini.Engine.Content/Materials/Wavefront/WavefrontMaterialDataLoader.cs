using System;
using System.Collections.Generic;
using Mini.Engine.IO;

namespace Mini.Engine.Content.Materials.Wavefront;

internal sealed class WavefrontMaterialDataLoader : IContentDataLoader<MaterialData>
{
    private readonly MtlStatementParser[] Parsers;
    private readonly IVirtualFileSystem FileSystem;

    public WavefrontMaterialDataLoader(IVirtualFileSystem fileSystem)
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
        this.FileSystem = fileSystem;
    }

    public MaterialData Load(ContentId id)
    {
        var state = new ParseState();
        var text = this.FileSystem.ReadAllText(id.Path).AsSpan();
        foreach (var line in text.EnumerateLines())
        {
            foreach (var parser in this.Parsers)
            {
                if (parser.Parse(state, line, this.FileSystem))
                {
                    break;
                }
            }
        }

        state.EndMaterial();
        return TransformToMaterialData(id, state);
    }

    private static MaterialData TransformToMaterialData(ContentId id, ParseState state)
    {
        var match = state.Materials.Find(m => m.Id.Equals(id.Key, StringComparison.InvariantCultureIgnoreCase))
            ?? throw new KeyNotFoundException($"Could not find material {id.Key} in material library {id.Path}");

        return match with { Id = id.ToString() };
    }
}
