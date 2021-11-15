namespace Mini.Engine.Content.Models.Obj;

/// <summary>
/// Specifies the material library file for the material definitions set with the usemtl statement.You can specify multiple filenames with mtllib.If multiple filenames are specified, the first file listed is searched first for the material definition, the second file is searched next, and so on.
/// syntax: mtllib filename1 filename2...
/// </summary>
internal sealed class MtlLibParser : StatementParser
{
    public override string Key => "mtllib";
    protected override void ParseArguments(ParseState state, SpanTokenEnumerator arguments)
    {
        foreach (var library in arguments)
        {
            state.MaterialLibraries.Add(library.ToString());
        }
    }
}
