using System;
using System.IO;
using Mini.Engine.Content.Models.Wavefront.Materials;
using Mini.Engine.IO;

namespace Mini.Engine.Content.Models.Wavefront.Objects;

/// <summary>
/// Specifies the material library file for the material definitions set with the usemtl statement.You can specify multiple filenames with mtllib.If multiple filenames are specified, the first file listed is searched first for the material definition, the second file is searched next, and so on.
/// syntax: mtllib filename1 filename2...
/// </summary>
internal sealed class MtlLibParser : ObjStatementParser
{
    public override string Key => "mtllib";

    private readonly MtlStatementParser[] Parsers;

    public MtlLibParser()
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

    protected override void ParseArguments(ObjectParseState state, SpanTokenEnumerator arguments, IVirtualFileSystem fileSystem)
    {
        foreach (var library in arguments)
        {
            var materialState = new MaterialParseState();
            var text = fileSystem.ReadAllText(Path.Combine(state.BasePath, new string(library))).AsSpan();
            foreach (var line in text.EnumerateLines())
            {
                foreach (var parser in this.Parsers)
                {
                    if (parser.Parse(materialState, line, fileSystem))
                    {
                        break;
                    }
                }
            }

            state.Materials.AddRange(materialState.Materials);
        }
    }
}
