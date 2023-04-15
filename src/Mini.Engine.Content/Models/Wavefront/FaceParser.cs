using Mini.Engine.Content.Parsers;
using Mini.Engine.IO;
using Vortice.Mathematics;

namespace Mini.Engine.Content.Models.Wavefront;

/// <summary>
/// Specifies a face element and its vertex reference number. You can optionally include the texture vertex and vertex normal reference numbers.
/// syntax: f v1/vt1/vn1 v2/vt2/vn2 v3/vt3/vn3..  or f v1//vn1 v2//vn2 v3//vn3  or f v1/vt1 v2/vt2 v3/vt3
/// </summary>
internal sealed class FaceParser : ObjStatementParser
{
    public override string Key => "f";

    protected override void ParseArguments(ParseState state, SpanTokenEnumerator arguments, IReadOnlyVirtualFileSystem fileSystem)
    {
        var face = new List<Int3>(4);
        foreach (var argument in arguments)
        {
            var triplet = ParseTriplet(argument);
            face.Add(triplet);
        }

        state.Faces.Add(face.ToArray());
    }

    private static Int3 ParseTriplet(ReadOnlySpan<char> slice)
    {
        var index = slice.IndexOf('/');
        var x  = int.Parse(slice[..index]);

        slice = slice[(index + 1)..];
        index = slice.IndexOf('/');
        var y = int.Parse(slice[..index]);
        var z = int.Parse(slice[(index + 1)..]);

        return new Int3(x, y, z);
    }
}
