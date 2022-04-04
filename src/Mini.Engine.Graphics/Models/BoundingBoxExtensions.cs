using System;
using System.Numerics;
using Vortice.Mathematics;

namespace Mini.Engine.Graphics.Models;
public static class BoundingBoxExtensions
{
    public static BoundingBox Transform(this BoundingBox box, in Matrix4x4 transform)
    {
        var size = box.Max - box.Min;
        var newCenter = Vector3.Transform(box.Center, transform);
        var oldEdge = size * 0.5f;

        var newEdge = new Vector3(
            Math.Abs(transform.M11) * oldEdge.X + Math.Abs(transform.M12) * oldEdge.Y + Math.Abs(transform.M13) * oldEdge.Z,
            Math.Abs(transform.M21) * oldEdge.X + Math.Abs(transform.M22) * oldEdge.Y + Math.Abs(transform.M23) * oldEdge.Z,
            Math.Abs(transform.M31) * oldEdge.X + Math.Abs(transform.M32) * oldEdge.Y + Math.Abs(transform.M33) * oldEdge.Z
        );

        return new(newCenter - newEdge, newCenter + newEdge);
    }
}
