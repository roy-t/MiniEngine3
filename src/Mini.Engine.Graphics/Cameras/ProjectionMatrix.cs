using System.Numerics;
using Mini.Engine.Core;

namespace Mini.Engine.Graphics.Cameras;

public static class ProjectionMatrix
{
    private static readonly QuasiRandomSequence Sequence = new QuasiRandomSequence(6);
    public static bool EnableJitter = false; // HACK

    // See https://thxforthefish.com/posts/reverse_z/

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

    // See: https://www.elopezr.com/temporal-aa-and-the-quest-for-the-holy-trail/

    public static Matrix4x4 Jitter(in Matrix4x4 projectionMatrix, float backBufferWidth, float backBufferHeigth)
    {
        var w = 2.0f * backBufferWidth;
        var h = 2.0f * backBufferHeigth;
        var factor = EnableJitter ? 1.0f : 0.0f;
        var jitter = Sequence.Next2D(-1.0f / w, 1.0f / w, -1.0f / h, 1.0f / h) * factor;

        var offset = new Matrix4x4
        {
            M11 = 1,
            M22 = 1,
            M33 = 1,
            M44 = 1,
            M41 = jitter.X,
            M42 = jitter.Y,            
        };     

        return projectionMatrix * offset;
    }
}
