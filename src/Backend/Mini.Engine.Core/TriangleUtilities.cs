using System.Diagnostics;
using System.Numerics;

namespace Mini.Engine.Core;
public static class TriangleUtilities
{

    /// <summary>
    /// Return the normal of the triangle, given the 3 vertices of a triangle. If the computed normal points towards you when
    /// seen such that the vertices a, b, c, are in clockwise order.
    /// </summary>
    public static Vector3 GetNormal(Vector3 a, Vector3 b, Vector3 c)
    {
        Debug.Assert(IsTriangle(a, b, c));

        var v1 = Vector3.Normalize(b - a);
        var v2 = Vector3.Normalize(c - a);

        return Vector3.Normalize(Vector3.Cross(v1, v2));
    }

    /// <summary>
    /// Numerically stable algorithm for calculating the area of a triangle, even needle like triangles. From: https://people.eecs.berkeley.edu/~wkahan/Triangle.pdf
    /// </summary>    
    public static float GetArea(Vector3 a, Vector3 b, Vector3 c)
    {
        Debug.Assert(IsTriangle(a, b, c));

        Span<float> distances = stackalloc float[3]
        {
            Vector3.Distance(a, b),
            Vector3.Distance(b, c),
            Vector3.Distance(c, a),
        };

        distances.Sort();

        var dX = distances[0];
        var dY = distances[1];
        var dZ = distances[2];

        return MathF.Sqrt((dX + (dY + dZ)) * (dZ - (dX - dY)) * (dZ + (dX - dY)) * (dX + (dY - dZ))) / 4.0f;
    }


    /// <summary>
    /// Returns true if the 3 vertices form a valid triangle. Returns false if the triangle is degenerate and the vertices form a line or point
    /// </summary>    
    public static bool IsTriangle(Vector3 a, Vector3 b, Vector3 c)
    {
        Span<float> distances = stackalloc float[3]
        {
            Vector3.Distance(a, b),
            Vector3.Distance(b, c),
            Vector3.Distance(c, a),
        };

        distances.Sort();

        return distances[0] + distances[1] > distances[2];
    }

    public static bool IsTriangleClockwise(Vector2 a, Vector2 b, Vector2 c)
    {
        var crossProduct = ((b.X - a.X) * (c.Y - a.Y)) - ((b.Y - a.Y) * (c.X - a.X));
        return crossProduct < 0;
    }

    public static bool IsTriangleCounterClockwise(Vector2 a, Vector2 b, Vector2 c)
    {
        return !IsTriangleClockwise(a, b, c);
    }
    public static bool IsTriangleClockwise(Vector3 a, Vector3 b, Vector3 c)
    {
        // TODO: double check if this is correct!
        var ab = new Vector3(b.X - a.X, b.Y - a.Y, b.Z - a.Z);
        var ac = new Vector3(c.X - a.X, c.Y - a.Y, c.Z - a.Z);
        var cross = Vector3.Cross(ab, ac);

        return cross.Z > 0;
    }

    public static bool IsTriangleCounterClockwise(Vector3 a, Vector3 b, Vector3 c)
    {
        return !IsTriangleClockwise(a, b, c);
    }

    public static bool IsVertexInsideTriangle(Vector2 vertex, Vector2 a, Vector2 b, Vector2 c)
    {
        var d1 = GetSide(vertex, a, b);
        var d2 = GetSide(vertex, b, c);
        var d3 = GetSide(vertex, c, a);

        var hasNegative = (d1 < 0) || (d2 < 0) || (d3 < 0);
        var hasPositive = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(hasNegative && hasPositive);
    }

    private static double GetSide(Vector2 a, Vector2 b, Vector2 c)
    {
        return ((b.X - a.X) * (c.Y - a.Y)) - ((b.Y - a.Y) * (c.X - a.X));
    }

    public static bool IsVertexInsideTriangle(Vector3 vertex, Vector3 a, Vector3 b, Vector3 c)
    {
        var barycentric = Barycentric(vertex, a, b, c);

        var alpha = barycentric.X;
        var beta = barycentric.Y;
        var gamma = barycentric.Z;

        return (0.0f <= alpha) && (alpha <= 1.0f) &&
               (0.0f <= beta) && (beta <= 1.0f) &&
               (0.0f <= gamma) && (gamma <= 1.0f);
    }

    /// <summary>
    /// Compute barycentric coordinates (u, v, w) for the given vertex with respect to triangle (a, b, c)
    /// </summary>    
    public static Vector3 Barycentric(Vector3 vertex, Vector3 a, Vector3 b, Vector3 c)
    {
        // https://gamedev.stackexchange.com/questions/23743/whats-the-most-efficient-way-to-find-barycentric-coordinates
        var v0 = b - a;
        var v1 = c - a;
        var v2 = vertex - a;

        var d00 = Vector3.Dot(v0, v0);
        var d01 = Vector3.Dot(v0, v1);
        var d11 = Vector3.Dot(v1, v1);
        var d20 = Vector3.Dot(v2, v0);
        var d21 = Vector3.Dot(v2, v1);
        var denom = d00 * d11 - d01 * d01;
        var v = (d11 * d20 - d01 * d21) / denom;
        var w = (d00 * d21 - d01 * d20) / denom;
        var u = 1.0f - v - w;

        return new Vector3(u, v, w);
    }
}
