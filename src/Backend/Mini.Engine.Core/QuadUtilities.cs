using System.Numerics;

namespace Mini.Engine.Core;
public static class QuadUtilities
{
    public static Vector3 GetNormal(Vector3 tr, Vector3 br, Vector3 bl, Vector3 tl)
    {
        var normalA = TriangleUtilities.GetNormal(tr, br, tl);
        var areaA = TriangleUtilities.GetArea(tr, br, tl);

        var normalB = TriangleUtilities.GetNormal(br, bl, tl);
        var areaB = TriangleUtilities.GetArea(br, bl, tl);

        return Vector3.Normalize((normalA * areaA) + (normalB * areaB));
    }
}
