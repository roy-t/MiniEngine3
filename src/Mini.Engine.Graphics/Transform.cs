using System;
using System.Numerics;
using Vortice.Mathematics;

namespace Mini.Engine.Graphics
{
    public sealed class Transform
    {
        public static readonly Transform Identity = new();

        public Transform()
            : this(Vector3.Zero) { }

        public Transform(Transform source)
        {
            this.Matrix = source.Matrix;
            this.Rotation = source.Rotation;
            this.Origin = source.Origin;
            this.Position = source.Position;
            this.Forward = source.Forward;
            this.Up = source.Up;
            this.Left = source.Left;
            this.Scale = source.Scale;
        }

        public Transform(Vector3 position)
            : this(position, Vector3.One, Quaternion.Identity) { }

        public Transform(Vector3 position, float scale)
            : this(position, Vector3.One * scale, Quaternion.Identity) { }

        public Transform(Vector3 position, Vector3 scale)
            : this(position, scale, Quaternion.Identity) { }

        public Transform(Vector3 position, Vector3 scale, Quaternion rotation, Vector3 origin = default)
        {
            this.Recompute(position, scale, rotation, origin);
        }

        public Matrix4x4 Matrix { get; private set; }
        public Quaternion Rotation { get; private set; }
        public Vector3 Origin { get; private set; }
        public Vector3 Position { get; private set; }
        public Vector3 Forward { get; private set; }
        public Vector3 Up { get; private set; }
        public Vector3 Left { get; private set; }
        public Vector3 Scale { get; private set; }

        public void MoveTo(Vector3 position)
        {
            this.Recompute(position, this.Scale, this.Rotation, this.Origin);
        }

        public void SetScale(float scale)
        {
            this.SetScale(Vector3.One * scale);
        }

        public void SetScale(Vector3 scale)
        {
            this.Recompute(this.Position, scale, this.Rotation, this.Origin);
        }

        public void SetOrigin(Vector3 origin)
        {
            this.Recompute(this.Position, this.Scale, this.Rotation, origin);
        }

        public void SetRotation(Quaternion rotation)
        {
            this.Recompute(this.Position, this.Scale, rotation, this.Origin);
        }

        public void ApplyRotation(Quaternion rotation)
        {
            this.Recompute(this.Position, this.Scale, rotation * this.Rotation, this.Origin);
        }

        public void FaceTarget(Vector3 target)
        {
            var newForward = Vector3.Normalize(target - this.Position);
            var rotation = GetRotation(this.Forward, newForward, this.Up);
            this.ApplyRotation(rotation);
        }

        public void FaceTargetConstrained(Vector3 target, Vector3 up)
        {
            var dot = Vector3.Dot(Vector3.Normalize(target - this.Position), up);
            if (Math.Abs(dot) < 0.99f)
            {
                var matrix = Matrix4x4.CreateLookAt(this.Position, target, up);
                if (Matrix4x4.Invert(matrix, out var inverted))
                {
                    var quaternion = Quaternion.CreateFromRotationMatrix(inverted);
                    this.SetRotation(quaternion);
                }
            }
        }

        private void Recompute(Vector3 position, Vector3 scale, Quaternion rotation, Vector3 origin = default)
        {
            this.Position = position;
            this.Scale = scale;
            this.Rotation = Quaternion.Normalize(rotation);
            this.Origin = origin;

            this.Forward = Vector3.Transform(-Vector3.UnitZ, rotation);
            this.Up = Vector3.Transform(Vector3.UnitY, rotation);
            this.Left = Vector3.Transform(-Vector3.UnitX, rotation);
            this.Matrix = Combine(position, scale, origin, rotation);
        }

        private static Matrix4x4 Combine(Vector3 position, Vector3 scale, Vector3 origin, Quaternion rotation)
        {
            var moveToCenter = Matrix4x4.CreateTranslation(-origin);
            var size = Matrix4x4.CreateScale(scale);
            var translation = Matrix4x4.CreateTranslation(position);

            var rotationMatrix4x4 = Matrix4x4.CreateFromQuaternion(rotation);
            return size * moveToCenter * rotationMatrix4x4 * translation;
        }

        private static Quaternion GetRotation(Vector3 currentForward, Vector3 desiredForward, Vector3 up)
        {
            var dot = Vector3.Dot(currentForward, desiredForward);

            if (Math.Abs(dot + 1.0f) < 0.000001f)
            {
                // vector a and b point exactly in the opposite direction, 
                // so it is a 180 degrees turn around the up-axis
                return new Quaternion(up, MathHelper.Pi);
            }
            if (Math.Abs(dot - 1.0f) < 0.000001f)
            {
                // vector a and b point exactly in the same direction
                // so we return the identity quaternion
                return Quaternion.Identity;
            }

            var rotAngle = (float)Math.Acos(dot);
            var rotAxis = Vector3.Cross(currentForward, desiredForward);
            rotAxis = Vector3.Normalize(rotAxis);
            return Quaternion.CreateFromAxisAngle(rotAxis, rotAngle);
        }
    }
}
