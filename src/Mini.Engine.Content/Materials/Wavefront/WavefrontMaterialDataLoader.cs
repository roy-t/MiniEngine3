using System;
using Mini.Engine.IO;

namespace Mini.Engine.Content.Materials.Wavefront;

internal class WavefrontMaterialDataLoader : IContentDataLoader<MaterialData>
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
        var library = id.Path;
        var key = id.Key;

        var text = this.FileSystem.ReadAllText(library).AsSpan();
        foreach (var line in text.EnumerateLines())
        {

        }
    }
}
