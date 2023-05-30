using System.Numerics;
using Mini.Engine.Graphics.Diesel;
using Mini.Engine.Modelling.Curves;
using Mini.Engine.Modelling.Paths;

namespace Mini.Engine.Modelling.Tools;
public static class Capper
{
    public static void Cap(IPrimitiveMeshPartBuilder partBuilder, ICurve curve, Path2D cap)
    {
        var normal = -Vector3.UnitZ;

        var triangles = EarClipping.Triangulate(cap.Positions);

        var startTransform = curve.AlignTo(0.0f, Vector3.UnitY);
        AddCap(partBuilder, cap, normal, startTransform, triangles);

        var endTransform = curve.Reverse().AlignTo(0.0f, Vector3.UnitY);
        AddCap(partBuilder, cap, normal, endTransform, triangles);
    }

    private static void AddCap(IPrimitiveMeshPartBuilder partBuilder, Path2D cap, Vector3 normal, Matrix4x4 transform, ReadOnlySpan<int> triangles)
    {
        var startIndex = int.MaxValue;
        foreach (var vertex in cap.Positions)
        {
            var v = Vector3.Transform(new Vector3(vertex.X, vertex.Y, 0.0f), transform);
            var n = Vector3.TransformNormal(normal, transform);
            var i = partBuilder.AddVertex(v, n);
            startIndex = Math.Min(startIndex, i);
        }

        foreach (var index in triangles)
        {
            partBuilder.AddIndex(index + startIndex);
        }
    }
}
