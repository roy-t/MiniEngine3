using System.Numerics;
using Mini.Engine.Core;

namespace Mini.Engine.Modelling.Tools;

public class EarClipping
{
    private readonly record struct IndexedVertex2D(int Index, Vector2 Vertex);

    private readonly record struct IndexedVertex3D(int Index, Vector3 Vertex);
    
    public static ReadOnlySpan<int> Triangulate(ReadOnlySpan<Vector3> polygon)
    {
        var indices = new ArrayBuilder<int>(polygon.Length * 2);
        var remaining = new List<IndexedVertex3D>(polygon.Length);

        for (var i = 0; i < polygon.Length; i++)
        {
            remaining.Add(new IndexedVertex3D(i, polygon[i]));
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

    /// <summary>
    /// Given a polygon without holes, defined by clockwise vertices, returns a triangulation of that polygon
    /// </summary>
    public static ReadOnlySpan<int> Triangulate(ReadOnlySpan<Vector2> polygon)
    {
        var indices = new ArrayBuilder<int>(polygon.Length * 2);
        var remaining = new List<IndexedVertex2D>(polygon.Length);

        for (var i = 0; i < polygon.Length; i++)
        {
            remaining.Add(new IndexedVertex2D(i, polygon[i]));
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

    private static bool IsEar(Vector2 v0, Vector2 v1, Vector2 v2, List<IndexedVertex2D> polygon)
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

    private static bool IsEar(Vector3 v0, Vector3 v1, Vector3 v2, List<IndexedVertex3D> polygon)
    {
        // TODO: doesn't make sense in 3D? Works fine without? 
        //if (TriangleUtilities.IsTriangleCounterClockwise(v0, v1, v2))
        //{
        //    return false;
        //}

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