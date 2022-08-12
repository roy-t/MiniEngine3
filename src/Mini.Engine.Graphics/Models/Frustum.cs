using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Vortice.Mathematics;

namespace Mini.Engine.Graphics.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Frustum
{
    private const int PlanesInACube = 6;

    private Plane P0;
    private Plane P1;
    private Plane P2;
    private Plane P3;
    private Plane P4;
    private Plane P5;

    public Frustum(Matrix4x4 viewProjection)
    {
        this.P0 = Plane.Normalize(new Plane(-viewProjection.M13, -viewProjection.M23, -viewProjection.M33, -viewProjection.M43));
        this.P1 = Plane.Normalize(new Plane(viewProjection.M13 - viewProjection.M14, viewProjection.M23 - viewProjection.M24, viewProjection.M33 - viewProjection.M34, viewProjection.M43 - viewProjection.M44));
        this.P2 = Plane.Normalize(new Plane(-viewProjection.M14 - viewProjection.M11, -viewProjection.M24 - viewProjection.M21, -viewProjection.M34 - viewProjection.M31, -viewProjection.M44 - viewProjection.M41));
        this.P3 = Plane.Normalize(new Plane(viewProjection.M11 - viewProjection.M14, viewProjection.M21 - viewProjection.M24, viewProjection.M31 - viewProjection.M34, viewProjection.M41 - viewProjection.M44));
        this.P4 = Plane.Normalize(new Plane(viewProjection.M12 - viewProjection.M14, viewProjection.M22 - viewProjection.M24, viewProjection.M32 - viewProjection.M34, viewProjection.M42 - viewProjection.M44));
        this.P5 = Plane.Normalize(new Plane(-viewProjection.M14 - viewProjection.M12, -viewProjection.M24 - viewProjection.M22, -viewProjection.M34 - viewProjection.M32, -viewProjection.M44 - viewProjection.M42));
    }

    private unsafe ref Plane this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref Unsafe.AsRef<Plane>(Unsafe.Add<Plane>(Unsafe.AsPointer(ref this.P0), index));
    }

    public bool ContainsOrIntersects(BoundingBox box)
    {        
        for (var i = 0; i < PlanesInACube; i++)
        {
            ref var plane = ref this[i];
            var intersection = box.Intersects(in plane);

            if (intersection == PlaneIntersectionType.Front)
            {
                return false;
            }
        }

        return true;
    }

    public bool ContainsOrIntersects(BoundingSphere sphere)
    {     
        for (var i = 0; i < PlanesInACube; i++)
        {
            ref var plane = ref this[i];
            var intersection = sphere.Intersects(in plane);

            if (intersection == PlaneIntersectionType.Front)
            {
                return false;
            }
        }

        return true;
    }
}