using System.Numerics;

namespace Mini.Engine.Graphics.Cameras;

public static class ProjectionMatrix
{
    // Based on https://thxforthefish.com/posts/reverse_z/

    public static Matrix4x4 InfiniteReversedZ(in PerspectiveCamera camera)
    {
        return InfiniteReversedZ(camera.NearPlane, camera.FieldOfView, camera.AspectRatio);
    }

    public static Matrix4x4 InfiniteReversedZ(float nearPlane, float fieldOfView, float aspectRatio)
    {
        var f = 1.0f / MathF.Tan(fieldOfView * 0.5f);
        return new Matrix4x4
        {
            M11 = f / aspectRatio,
            M22 = f,
            M34 = -1.0f,
            M43 = nearPlane
        };
    }

    public static Matrix4x4 ReversedZ(in PerspectiveCamera camera)
    {
        return ReversedZ(camera.NearPlane, camera.FarPlane, camera.FieldOfView, camera.AspectRatio);
    }

    public static Matrix4x4 ReversedZ(float nearPlane, float farPlane, float fieldOfView, float aspectRatio)
    {
        var f = 1.0f / MathF.Tan(fieldOfView * 0.5f);
        return new Matrix4x4
        {
            M11 = f / aspectRatio,
            M22 = f,
            M33 = nearPlane / (farPlane - nearPlane),
            M34 = -1.0f,
            M43 = farPlane * nearPlane / (farPlane - nearPlane)
        };
    }
}
