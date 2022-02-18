using System.Numerics;
using Vortice.Mathematics;

namespace Mini.Engine.Graphics.Models;
public sealed class Frustum
{
    private readonly Plane[] Planes;

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
    }

    public bool ContainsOrIntersects(BoundingBox box)
    {
        for (var i = 0; i < this.Planes.Length; i++)
        {
            var plane = this.Planes[i];
            var intersection = box.Intersects(ref plane);

            if (intersection == PlaneIntersectionType.Intersecting)
            {
                return true;
            }

            if (intersection == PlaneIntersectionType.Front)
            {
                return false;
            }
        }

        return true;
    }
}
