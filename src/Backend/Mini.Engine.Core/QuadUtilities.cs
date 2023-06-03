using System.Numerics;
using LibGame.Geometry;

namespace Mini.Engine.Core;
public static class QuadUtilities
{
    public static Vector3 GetNormal(Vector3 tr, Vector3 br, Vector3 bl, Vector3 tl)
    {
        var normalA = Triangles.GetNormal(tr, br, tl);
        var areaA = Triangles.GetArea(tr, br, tl);

        var normalB = Triangles.GetNormal(br, bl, tl);
        var areaB = Triangles.GetArea(br, bl, tl);

        return Vector3.Normalize((normalA * areaA) + (normalB * areaB));
    }
}
