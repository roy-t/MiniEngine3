using System;
using System.Collections.Generic;
using Mini.Engine.Content.Textures;
using Mini.Engine.DirectX;
using Mini.Engine.IO;

namespace Mini.Engine.Content.Materials.Wavefront;

internal sealed partial class WavefrontMaterialDataLoader : IContentDataLoader<MaterialData>
{
    private static readonly ContentId Albedo = new(@"Materials\albedo.tga");
    private static readonly ContentId Metalicness = new(@"Materials\metalicness.tga");
    private static readonly ContentId Normal = new(@"Materials\normal.tga");
    private static readonly ContentId Roughness = new(@"Materials\roughness.tga");
    private static readonly ContentId AmbientOcclusion = new(@"Materials\ao.tga");

    private readonly MtlStatementParser[] Parsers;
    private readonly IContentLoader<Texture2DContent> TextureLoader;
    private readonly IVirtualFileSystem FileSystem;

    public WavefrontMaterialDataLoader(IVirtualFileSystem fileSystem, IContentLoader<Texture2DContent> textureLoader)
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
        this.TextureLoader = textureLoader;
        this.FileSystem = fileSystem;
    }

    public MaterialData Load(Device device, ContentId id)
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
            this.TextureLoader.Load(device, GetIdOrFallback(id, MaterialType.Albedo, record.Albedo)),
            this.TextureLoader.Load(device, GetIdOrFallback(id, MaterialType.Metalicness, record.Metalicness)),
            this.TextureLoader.Load(device, GetIdOrFallback(id, MaterialType.Normal, record.Normal)),
            this.TextureLoader.Load(device, GetIdOrFallback(id, MaterialType.Roughness, record.Roughness)),
            this.TextureLoader.Load(device, GetIdOrFallback(id, MaterialType.AmbientOcclusion, record.AmbientOcclusion)));
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
