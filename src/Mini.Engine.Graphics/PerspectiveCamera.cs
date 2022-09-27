using System.Numerics;

namespace Mini.Engine.Graphics;

public readonly record struct PerspectiveCamera(float NearPlane, float FarPlane, float FieldOfView, float AspectRatio)
{ 
    // https://thxforthefish.com/posts/reverse_z/

    public Matrix4x4 GetBoundedReversedZViewProjection(in Transform transform)
    {
        var view = Matrix4x4.CreateLookAt(transform.GetPosition(), transform.GetPosition() + transform.GetForward(), transform.GetUp());
        
        var f = 1.0f / MathF.Tan(this.FieldOfView * 0.5f);
        var proj = new Matrix4x4
        {
            M11 = f / this.AspectRatio,
            M22 = f,
            M33 = this.NearPlane / (this.FarPlane - this.NearPlane),
            M34 = -1.0f,
            M43 = (this.FarPlane * this.NearPlane) / (this.FarPlane - this.NearPlane)            
        };

        return view * proj;
    }
    
    public Matrix4x4 GetInfiniteReversedZViewProjection(in Transform transform)
    {
        var view = Matrix4x4.CreateLookAt(transform.GetPosition(), transform.GetPosition() + transform.GetForward(), transform.GetUp());

        var proj = CreateInfiniteReversedZProjectionMatrix(this.NearPlane, this.FieldOfView, this.AspectRatio);

        return view * proj;
    }

    public static Matrix4x4 CreateInfiniteReversedZProjectionMatrix(float nearPlane, float fieldOfView, float aspectRatio)
    {
        var f = 1.0f / MathF.Tan(fieldOfView * 0.5f);
        var proj = new Matrix4x4
        {
            M11 = f / aspectRatio,
            M22 = f,
            M34 = -1.0f,
            M43 = nearPlane
        };
        return proj;
    }
}
