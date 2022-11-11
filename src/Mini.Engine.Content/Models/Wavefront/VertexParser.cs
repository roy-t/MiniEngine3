using System.Globalization;
using System.Numerics;
using Mini.Engine.Content.Parsers;
using Mini.Engine.IO;

namespace Mini.Engine.Content.Models.Wavefront;

internal abstract class VertexParser : ObjStatementParser
{
    private static readonly IFormatProvider FloatFormat = CultureInfo.InvariantCulture.NumberFormat;

    protected override void ParseArguments(ParseState state, SpanTokenEnumerator arguments, IReadOnlyVirtualFileSystem fileSystem)
    {
        var elements = new float[4];
        var index = 0;
        foreach (var argument in arguments)
        {

            elements[index++] = float.Parse(argument, NumberStyles.Float, FloatFormat);
        }

        var vector = new Vector4(elements);
        ParseVertex(state, vector);

    }

    protected abstract void ParseVertex(ParseState state, Vector4 vertex);
}
