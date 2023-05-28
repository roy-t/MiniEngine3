using System.Numerics;
using Mini.Engine.Core;

namespace Mini.Engine.Modelling.Tools;

public class EarClipping
{
    private readonly record struct IndexedVertex(int Index, Vector2 Vertex);

    /// <summary>
    /// Given a polygon without holes, defined by clockwise vertices, returns a triangulation of that polygon
    /// </summary>
    public static ReadOnlySpan<int> Triangulate(ReadOnlySpan<Vector2> polygon)
    {
        var indices = new ArrayBuilder<int>(polygon.Length * 2);
        var remaining = new List<IndexedVertex>(polygon.Length);

        for (var i = 0; i < polygon.Length; i++)
        {
            remaining.Add(new IndexedVertex(i, polygon[i]));
        }

        while (remaining.Count >= 3)
        {
            var n = remaining.Count;
            for (var i = 0; i < n; i++)
            {
                (var i0, var v0) = remaining[(i + 0) % n];
                (var i1, var v1) = remaining[(i + 1) % n];
                (var i2, var v2) = remaining[(i + 2) % n];

                if (IsEar(v0, v1, v2, remaining))
                {
                    indices.Add(i0);
                    indices.Add(i1);
                    indices.Add(i2);

                    remaining.RemoveAt((i + 1) % n);

                    break;
                }
            }
        }

        return indices.Build();
    }

    private static bool IsEar(Vector2 v0, Vector2 v1, Vector2 v2, List<IndexedVertex> polygon)
    {
        if (TriangleUtilities.IsTriangleCounterClockwise(v0, v1, v2))
        {
            return false;
        }

        foreach ((var _, var vertex) in polygon)
        {
            if (vertex != v0 && vertex != v1 && vertex != v2)
            {
                if (TriangleUtilities.IsVertexInsideTriangle(vertex, v0, v1, v2))
                {
                    return false;
                }
            }
        }

        return true;
    }   
}