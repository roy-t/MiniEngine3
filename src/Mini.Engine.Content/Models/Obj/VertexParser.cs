using System;
using System.Globalization;
using System.Numerics;

namespace Mini.Engine.Content.Models.Obj;

internal abstract class VertexParser : StatementParser
{
    private static readonly IFormatProvider FloatFormat = CultureInfo.InvariantCulture.NumberFormat;

    protected override void ParseArguments(ParseState state, SpanTokenEnumerator arguments)
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
