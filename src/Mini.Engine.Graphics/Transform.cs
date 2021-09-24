using System;
using System.Numerics;
using Vortice.Mathematics;

namespace Mini.Engine.Graphics
{
    public readonly struct Transform
    {
        public static readonly Transform Identity = new();

        public readonly Matrix4x4 Matrix;
        public readonly Quaternion Rotation;
        public readonly Vector3 Origin;
        public readonly Vector3 Position;
        public readonly Vector3 Forward;
        public readonly Vector3 Up;
        public readonly Vector3 Left;
        public readonly Vector3 Scale;

        public Transform()
            : this(Vector3.Zero) { }

        public Transform(Transform source)
            : this(source.Matrix, source.Rotation, source.Origin, source.Position, source.Forward, source.Up, source.Left, source.Scale) { }

        public Transform(Vector3 position)
            : this(position, Vector3.One, Quaternion.Identity) { }

        public Transform(Vector3 position, float scale)
            : this(position, Vector3.One * scale, Quaternion.Identity) { }

        public Transform(Vector3 position, Vector3 scale)
            : this(position, scale, Quaternion.Identity) { }

        public Transform(Vector3 position, Vector3 scale, Quaternion rotation, Vector3 origin = default)
            : this(Recompute(position, scale, rotation, origin)) { }

        private Transform(Matrix4x4 matrix, Quaternion rotation, Vector3 origin, Vector3 position, Vector3 forward, Vector3 up, Vector3 left, Vector3 scale)
        {
            this.Matrix = matrix;
            this.Rotation = rotation;
            this.Origin = origin;
            this.Position = position;
            this.Forward = forward;
            this.Up = up;
            this.Left = left;
            this.Scale = scale;
        }

        public Transform MoveTo(Vector3 position)
        {
            return Recompute(position, this.Scale, this.Rotation, this.Origin);
        }

        public Transform SetScale(float scale)
        {
            return this.SetScale(Vector3.One * scale);
        }

        public Transform SetScale(Vector3 scale)
        {
            return Recompute(this.Position, scale, this.Rotation, this.Origin);
        }

        public Transform SetOrigin(Vector3 origin)
        {
            return Recompute(this.Position, this.Scale, this.Rotation, origin);
        }

        public Transform SetRotation(Quaternion rotation)
        {
            return Recompute(this.Position, this.Scale, rotation, this.Origin);
        }

        public Transform ApplyRotation(Quaternion rotation)
        {
            return Recompute(this.Position, this.Scale, rotation * this.Rotation, this.Origin);
        }

        public Transform FaceTarget(Vector3 target)
        {
            var newForward = Vector3.Normalize(target - this.Position);
            var rotation = GetRotation(this.Forward, newForward, this.Up);
            return this.ApplyRotation(rotation);
        }

        public Transform FaceTargetConstrained(Vector3 target, Vector3 up)
        {
            var dot = Vector3.Dot(Vector3.Normalize(target - this.Position), up);
            if (Math.Abs(dot) < 0.99f)
            {
                var matrix = Matrix4x4.CreateLookAt(this.Position, target, up);
                if (Matrix4x4.Invert(matrix, out var inverted))
                {
                    var quaternion = Quaternion.CreateFromRotationMatrix(inverted);
                    return this.SetRotation(quaternion);
                }
            }

            return this;
        }

        private static Transform Recompute(Vector3 position, Vector3 scale, Quaternion rotation, Vector3 origin = default)
        {
            rotation = Quaternion.Normalize(rotation);

            var forward = Vector3.Transform(-Vector3.UnitZ, rotation);
            var up = Vector3.Transform(Vector3.UnitY, rotation);
            var left = Vector3.Transform(-Vector3.UnitX, rotation);
            var matrix = Combine(position, scale, origin, rotation);

            return new Transform(matrix, rotation, origin, position, forward, up, left, scale);
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
