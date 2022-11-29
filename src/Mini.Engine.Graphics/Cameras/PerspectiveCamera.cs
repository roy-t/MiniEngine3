using System.Numerics;

namespace Mini.Engine.Graphics.Cameras;

public readonly record struct PerspectiveCamera(float NearPlane, float FarPlane, float FieldOfView, float AspectRatio)
{
    public Matrix4x4 GetViewProjection(in Transform transform)
    {
        var view = Matrix4x4.CreateLookAt(transform.GetPosition(), transform.GetPosition() + transform.GetForward(), transform.GetUp());
        var proj = Matrix4x4.CreatePerspectiveFieldOfView(this.FieldOfView, this.AspectRatio, this.NearPlane, this.FarPlane);

        return view * proj;
    }
    
    public Matrix4x4 GetBoundedReversedZViewProjection(in Transform transform)
    {
        var view = Matrix4x4.CreateLookAt(transform.GetPosition(), transform.GetPosition() + transform.GetForward(), transform.GetUp());
        var proj = ProjectionMatrix.ReversedZ(in this);
        
        return view * proj;
    }

    public Matrix4x4 GetInfiniteReversedZViewProjection(in Transform transform, Vector2 jitter)
    {
        var view = Matrix4x4.CreateLookAt(transform.GetPosition(), transform.GetPosition() + transform.GetForward(), transform.GetUp());
        var proj = ProjectionMatrix.InfiniteReversedZ(in this);
        proj = ProjectionMatrix.Jitter(in proj, jitter);
        return view * proj;
    }
}
