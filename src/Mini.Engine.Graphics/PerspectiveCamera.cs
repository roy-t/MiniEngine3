using System.Numerics;

namespace Mini.Engine.Graphics
{
    public readonly struct PerspectiveCamera : ITransformable<PerspectiveCamera>
    {
        public const float NearPlane = 0.1f;
        public const float FarPlane = 250.1f;
        public const float FieldOfView = MathF.PI / 2.0f;

        public readonly Matrix4x4 ViewProjection;
        public readonly float AspectRatio;

        public PerspectiveCamera(float aspectRatio, Transform transform)
        {
            this.Transform = transform;
            this.AspectRatio = aspectRatio;

            var view = Matrix4x4.CreateLookAt(transform.Position, transform.Position + transform.Forward, transform.Up);
            var proj = Matrix4x4.CreatePerspectiveFieldOfView(FieldOfView, aspectRatio, NearPlane, FarPlane);

            this.ViewProjection = view * proj;
        }

        public readonly Transform Transform { get; }

        public PerspectiveCamera Retransform(Transform transform)
        {
            return new PerspectiveCamera(this.AspectRatio, transform);
        }
    }
}
