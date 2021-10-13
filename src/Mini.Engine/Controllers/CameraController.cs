using System;
using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Graphics;
using Mini.Engine.Input;
using Vortice.DirectInput;

namespace Mini.Engine.Controllers
{
    [Service]
    public sealed class CameraController
    {
        private readonly KeyboardController Keyboard;
        private readonly MouseController Mouse;
        private readonly float LinearVelocity = 1.1f;
        private readonly float AngularVelocity = MathF.PI * 0.002f;

        public CameraController(KeyboardController keyboard, MouseController mouse)
        {
            this.Keyboard = keyboard;
            this.Mouse = mouse;
        }

        public void Update(ref PerspectiveCamera camera, float elapsed)
        {
            var step = elapsed * this.LinearVelocity;

            var forward = camera.Transform.Forward;
            var backward = -forward;
            var up = camera.Transform.Up;
            var down = -up;
            var left = camera.Transform.Left;
            var right = -left;

            var groundMovement = this.Keyboard.AsVector(InputState.Pressed, Key.W, Key.A, Key.S, Key.D);
            var verticalMovement = this.Keyboard.AsVector(InputState.Pressed, Key.Space, Key.LeftControl);

            if (groundMovement.LengthSquared() > 0 || verticalMovement.LengthSquared() > 0)
            {
                var translation = Vector3.Zero;
                translation += groundMovement.X * forward;
                translation += groundMovement.Y * left;
                translation += groundMovement.Z * backward;
                translation += groundMovement.W * right;

                translation += verticalMovement.X * up;
                translation += verticalMovement.Y * down;

                translation *= step;
                camera = camera.ApplyTranslation(translation);
            }

            var movement = this.Mouse.Movement * this.AngularVelocity;
            if (movement.LengthSquared() > 0 && this.Mouse.Held(2))
            {
                var rotation = Quaternion.Identity;
                rotation *= Quaternion.CreateFromAxisAngle(up, movement.X);
                rotation *= Quaternion.CreateFromAxisAngle(right, movement.Y);

                var lookAt = camera.Transform.Position + Vector3.Transform(camera.Transform.Forward, rotation);
                camera = camera.FaceTargetConstrained(lookAt, Vector3.UnitY);
            }
        }
    }
}
