using System;
using System.Collections.Generic;
using Mini.Engine.Content.Textures;
using Mini.Engine.DirectX;
using Mini.Engine.IO;

namespace Mini.Engine.Content.Materials.Wavefront;

internal sealed class WavefrontMaterialDataLoader : IContentDataLoader<MaterialData>
{
    private readonly MtlStatementParser[] Parsers;
    private readonly Device Device;
    private readonly IContentLoader<Texture2DContent> TextureLoader;
    private readonly IVirtualFileSystem FileSystem;

    public WavefrontMaterialDataLoader(Device device, IContentLoader<Texture2DContent> textureLoader, IVirtualFileSystem fileSystem)
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
        this.Device = device;
        this.TextureLoader = textureLoader;
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
        var record = GetRecord(id, state);

        return new MaterialData(id,
            this.TextureLoader.Load(this.Device, id.RelativeTo(record.Albedo)),
            this.TextureLoader.Load(this.Device, id.RelativeTo(record.Metalicness)),
            this.TextureLoader.Load(this.Device, id.RelativeTo(record.Normal)),
            this.TextureLoader.Load(this.Device, id.RelativeTo(record.Roughness)),
            this.TextureLoader.Load(this.Device, id.RelativeTo(record.AmbientOcclusion)));
    }

    private static MaterialRecords GetRecord(ContentId id, ParseState state)
    {
        var match = state.Materials.Find(m => id.Key.Equals(m.Key, StringComparison.InvariantCultureIgnoreCase))
            ?? throw new KeyNotFoundException($"Could not find material {id.Key} in material library {id.Path}");

        return match;
    }
}
