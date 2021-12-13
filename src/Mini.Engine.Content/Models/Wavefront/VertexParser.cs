using System;
using System.Globalization;
using System.Numerics;
using Mini.Engine.IO;

namespace Mini.Engine.Content.Models.Wavefront.Objects;

internal abstract class VertexParser : ObjStatementParser
{
    private static readonly IFormatProvider FloatFormat = CultureInfo.InvariantCulture.NumberFormat;

    protected override void ParseArguments(ObjectParseState state, SpanTokenEnumerator arguments, IVirtualFileSystem fileSystem)
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

    protected abstract void ParseVertex(ObjectParseState state, Vector4 vertex);
}
