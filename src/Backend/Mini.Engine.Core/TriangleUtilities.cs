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
}
