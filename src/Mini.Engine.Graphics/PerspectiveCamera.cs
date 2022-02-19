using System;
using System.Numerics;
using Mini.Engine.Graphics.Models;
using Mini.Engine.Graphics.Transforms;

namespace Mini.Engine.Graphics;

public sealed class PerspectiveCamera : ITransformable<PerspectiveCamera>
{
    public const float NearPlane = 0.1f;
    public const float FarPlane = 250.0f;
    public const float FieldOfView = MathF.PI / 2.0f;

    public readonly float AspectRatio;

    public PerspectiveCamera(float aspectRatio, Transform transform)
    {
        this.Transform = transform;
        this.AspectRatio = aspectRatio;        
        this.ComputeMatrices();

        this.Frustum = new Frustum(this.ViewProjection);
    }

    public Matrix4x4 ViewProjection { get; private set; }

    public Transform Transform { get; }

    public Frustum Frustum { get; private set; }

    public PerspectiveCamera OnTransform()
    {
        this.ComputeMatrices();
        this.Frustum = new Frustum(this.ViewProjection);
        return this;
    }

    private void ComputeMatrices()
    {
        var view = Matrix4x4.CreateLookAt(this.Transform.Position, this.Transform.Position + this.Transform.Forward, this.Transform.Up);
        var proj = Matrix4x4.CreatePerspectiveFieldOfView(FieldOfView, this.AspectRatio, NearPlane, FarPlane);

        this.ViewProjection = view * proj;
    }
}
