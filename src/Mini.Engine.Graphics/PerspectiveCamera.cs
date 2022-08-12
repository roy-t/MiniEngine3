using System.Numerics;
using Mini.Engine.Graphics.Models;
using Mini.Engine.Graphics.Transforms;

namespace Mini.Engine.Graphics;

// TODO: convert to struct, or not?
public sealed class PerspectiveCamera
{
    public PerspectiveCamera(float aspectRatio, Transform transform)
    {
        this.Transform = transform;
        this.AspectRatio = aspectRatio;
    }

    public float NearPlane { get; } = 0.25f;
    public float FarPlane { get; } = 250.0f;
    public float FieldOfView { get; } = MathF.PI / 2.0f;
    public float AspectRatio { get; }

    public Transform Transform { get; set; }

    public Matrix4x4 GetViewProjection(Transform transform)
    {
        var view = Matrix4x4.CreateLookAt(transform.GetPosition(), transform.GetPosition() + transform.GetForward(), transform.GetUp());
        var proj = Matrix4x4.CreatePerspectiveFieldOfView(this.FieldOfView, this.AspectRatio, this.NearPlane, this.FarPlane);

        return view * proj;
    }
}
