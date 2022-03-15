using System.Numerics;
using Vortice.Mathematics;

namespace Mini.Engine.Graphics.Models;
public sealed class Frustum
{
    public Frustum(Matrix4x4 viewProjection)
    {
        this.Planes = new Plane[]
        {
            Plane.Normalize(new Plane(-viewProjection.M13, -viewProjection.M23, -viewProjection.M33, -viewProjection.M43)),
            Plane.Normalize(new Plane(viewProjection.M13 - viewProjection.M14, viewProjection.M23 - viewProjection.M24, viewProjection.M33 - viewProjection.M34, viewProjection.M43 - viewProjection.M44)),
            Plane.Normalize(new Plane(-viewProjection.M14 - viewProjection.M11, -viewProjection.M24 - viewProjection.M21, -viewProjection.M34 - viewProjection.M31, -viewProjection.M44 - viewProjection.M41)),
            Plane.Normalize(new Plane(viewProjection.M11 - viewProjection.M14, viewProjection.M21 - viewProjection.M24, viewProjection.M31 - viewProjection.M34, viewProjection.M41 - viewProjection.M44)),
            Plane.Normalize(new Plane(viewProjection.M12 - viewProjection.M14, viewProjection.M22 - viewProjection.M24, viewProjection.M32 - viewProjection.M34, viewProjection.M42 - viewProjection.M44)),
            Plane.Normalize(new Plane(-viewProjection.M14 - viewProjection.M12, -viewProjection.M24 - viewProjection.M22, -viewProjection.M34 - viewProjection.M32, -viewProjection.M44 - viewProjection.M42)),
        };

        this.Corners = new Vector3[]
        {
            IntersectionPoint(this.Planes[0], this.Planes[2], this.Planes[4]),
            IntersectionPoint(this.Planes[0], this.Planes[3], this.Planes[4]),
            IntersectionPoint(this.Planes[0], this.Planes[3], this.Planes[5]),
            IntersectionPoint(this.Planes[0], this.Planes[2], this.Planes[5]),
            IntersectionPoint(this.Planes[1], this.Planes[2], this.Planes[4]),
            IntersectionPoint(this.Planes[1], this.Planes[3], this.Planes[4]),
            IntersectionPoint(this.Planes[1], this.Planes[3], this.Planes[5]),
            IntersectionPoint(this.Planes[1], this.Planes[2], this.Planes[5]),
        };
    }

    public Plane[] Planes { get; }
    public Vector3[] Corners { get; }

    public bool ContainsOrIntersects(BoundingBox box)
    {
        for (var i = 0; i < this.Planes.Length; i++)
        {
            var plane = this.Planes[i];
            var intersection = box.Intersects(ref plane);

            if (intersection == PlaneIntersectionType.Front)
            {
                return false;
            }
        }

        return true;
    }

    public bool ContainsOrIntersects(BoundingSphere sphere)
    {
        for (var i = 0; i < this.Planes.Length; i++)
        {
            var plane = this.Planes[i];
            var intersection = sphere.Intersects(in plane);

            if (intersection == PlaneIntersectionType.Front)
            {
                return false;
            }
        }

        return true;
    }

    private static Vector3 IntersectionPoint(Plane a, Plane b, Plane c)
    {
        var cross = Vector3.Cross(b.Normal, c.Normal);

        var f = Vector3.Dot(a.Normal, cross);
        f *= -1.0f;

        var v1 = Vector3.Multiply(cross, a.D);

        cross = Vector3.Cross(c.Normal, a.Normal);
        var v2 = Vector3.Multiply(cross, b.D);

        cross = Vector3.Cross(a.Normal, b.Normal);
        var v3 = Vector3.Multiply(cross, c.D);

        Vector3 result;
        result.X = (v1.X + v2.X + v3.X) / f;
        result.Y = (v1.Y + v2.Y + v3.Y) / f;
        result.Z = (v1.Z + v2.Z + v3.Z) / f;

        return result;
    }
}
