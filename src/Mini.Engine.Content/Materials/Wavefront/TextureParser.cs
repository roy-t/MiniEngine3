using System;
using Mini.Engine.IO;

namespace Mini.Engine.Content.Materials.Wavefront;

internal abstract class TextureParser : MtlStatementParser
{
    private readonly Action<ParseState, string> Assign;
    private readonly string TextureKey;

    protected TextureParser(string key, Action<ParseState, string> assign)
    {
        this.Assign = assign;
        this.TextureKey = key;
    }

    public override string Key => this.TextureKey;

    protected override void ParseArgument(ParseState state, ReadOnlySpan<char> argument, IVirtualFileSystem fileSystem)
    {
        var name = new string(argument);
        this.Assign(state, name);
    }
}

// TODO: give up on mapping PBR materials to OBJ and invent our own keywords
// See also: http://scylardor.fr/2021/05/21/coercing-assimp-into-reading-obj-pbr-materials/
// See also: http://www.paulbourke.net/dataformats/mtl/

internal class AlbedoParser : TextureParser
{
    // Originally diffuse
    public AlbedoParser()
        : base("map_Kd", (s, t) => s.Albedo = t)
    {
    }
}

internal class MetalicnessParser : TextureParser
{
    // Originally ambient reflectivity
    public MetalicnessParser()
        : base("map_Ka", (s, t) => s.Metalicness = t)
    {
    }
}

internal class NormalParser : TextureParser
{
    // Originally height
    public NormalParser()
        : base("map_bump", (s, t) => s.Normal = t)
    {
    }
}

internal class RoughnessParser : TextureParser
{
    // Originally specular exponent
    public RoughnessParser()
        : base("map_Ns", (s, t) => s.Roughness = t)
    {
    }
}

internal class AmbientOcclusionParser : TextureParser
{
    // Originally emissive
    public AmbientOcclusionParser()
        : base("map_Ke", (s, t) => s.AmbientOcclusion = t)
    {
    }
}
