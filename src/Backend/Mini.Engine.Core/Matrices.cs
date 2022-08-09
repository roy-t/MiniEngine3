using System.Numerics;

namespace Mini.Engine.Core;
public static class Matrices
{
    public static Matrix4x4 CreateRowMajor(Vector4 r0, Vector4 r1, Vector4 r2, Vector4 r3)
    {
        return new Matrix4x4(r0.X, r0.Y, r0.Z, r0.W, r1.X, r1.Y, r1.Z, r1.W, r2.X, r2.Y, r2.Z, r2.W, r3.X, r3.Y, r3.Z, r3.W);
    }

    public static Matrix4x4 CreateColumnMajor(Vector4 r0, Vector4 r1, Vector4 r2, Vector4 r3)
    {
        return new Matrix4x4(r0.X, r1.X, r2.X, r3.X, r0.Y, r1.Y, r2.Y, r3.Y, r0.Z, r1.Z, r2.Z, r3.Z, r0.W, r1.W, r2.W, r3.W);
    }
}
