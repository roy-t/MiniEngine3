using System.Numerics;
using Mini.Engine.Core;

namespace Mini.Engine.Graphics.Cameras;

public static class ProjectionMatrix
{        
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
    
    public static Matrix4x4 Jitter(in Matrix4x4 projectionMatrix, Vector2 jitter)
    {        
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
