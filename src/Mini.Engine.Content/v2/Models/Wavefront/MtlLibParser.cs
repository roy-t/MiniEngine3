using System;
using Mini.Engine.IO;

namespace Mini.Engine.Content.v2.Models.Wavefront;

/// <summary>
/// Specifies the material library file for the material definitions set with the usemtl statement.You can specify multiple filenames with mtllib.If multiple filenames are specified, the first file listed is searched first for the material definition, the second file is searched next, and so on.
/// syntax: mtllib filename1 filename2...
/// </summary>
internal sealed class MtlLibParser : ObjStatementParser
{
    public override string Key => "mtllib";

    protected override void ParseArgument(ParseState state, ReadOnlySpan<char> argument, IReadOnlyVirtualFileSystem fileSystem)
    {
        state.MaterialLibrary = new string(argument);
    }
}