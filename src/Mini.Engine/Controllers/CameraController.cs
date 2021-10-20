using System;
using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Graphics;
using Mini.Engine.Windows;
using Vortice.Win32;

namespace Mini.Engine.Controllers
{
    [Service]
    public sealed class CameraController
    {
        private static readonly ushort CodeLeft = RawInputController.GetScanCode(VK.KEY_A);
        private static readonly ushort CodeRight = RawInputController.GetScanCode(VK.KEY_D);
        private static readonly ushort CodeForward = RawInputController.GetScanCode(VK.KEY_W);
        private static readonly ushort CodeBackward = RawInputController.GetScanCode(VK.KEY_S);
        private static readonly ushort CodeUp = RawInputController.GetScanCode(VK.SPACE);
        private static readonly ushort CodeDown = RawInputController.GetScanCode(VK.KEY_C);

        private const float MinLinearVelocity = 1.0f;
        private const float MaxLinearVelocity = 25.0f;

        private readonly float AngularVelocity = MathF.PI * 0.002f;

        private float linearVelocity = 5.0f;

        private readonly Mouse Mouse;
        private readonly Keyboard Keyboard;
        private readonly RawInputController InputController;

        public CameraController(RawInputController inputController)
        {
            this.Mouse = new Mouse();
            this.Keyboard = new Keyboard();
            this.InputController = inputController;
        }

        public void Update(ref PerspectiveCamera camera, float elapsed)
        {
            var horizontal = Vector4.Zero;
            var vertical = Vector2.Zero;

            while (this.InputController.ProcessEvents(this.Keyboard))
            {
                horizontal += this.Keyboard.AsVector(InputState.Pressed, CodeForward, CodeLeft, CodeBackward, CodeRight);
                vertical += this.Keyboard.AsVector(InputState.Pressed, CodeUp, CodeDown);
            }

            if (horizontal.LengthSquared() > 0 || vertical.LengthSquared() > 0)
            {
                var step = elapsed * this.linearVelocity;

                var forward = camera.Transform.Forward;
                var backward = -forward;
                var up = camera.Transform.Up;
                var down = -up;
                var left = camera.Transform.Left;
                var right = -left;

                var translation = Vector3.Zero;
                translation += horizontal.X * forward;
                translation += horizontal.Y * left;
                translation += horizontal.Z * backward;
                translation += horizontal.W * right;

                translation += vertical.X * up;
                translation += vertical.Y * down;

                translation *= step;
                camera = camera.ApplyTranslation(translation);
            }

            var movement = Vector2.Zero;
            var scrolledUp = false;
            var scrolledDown = false;
            var rightButtonDown = false;

            while (this.InputController.ProcessEvents(this.Mouse))
            {
                movement += this.Mouse.Movement;
                scrolledUp = this.Mouse.ScrolledUp;
                scrolledDown = this.Mouse.ScrolledDown;
                rightButtonDown = this.Mouse.Held(MouseButtons.Right);
            }

            if (movement.LengthSquared() > 0 && rightButtonDown)
            {
                movement *= this.AngularVelocity;
                var rotation = Quaternion.Identity;
                rotation *= Quaternion.CreateFromAxisAngle(camera.Transform.Up, movement.X);
                rotation *= Quaternion.CreateFromAxisAngle(-camera.Transform.Left, movement.Y);

                var lookAt = camera.Transform.Position + Vector3.Transform(camera.Transform.Forward, rotation);
                camera = camera.FaceTargetConstrained(lookAt, Vector3.UnitY);
            }

            if (scrolledUp)
            {
                this.linearVelocity = Math.Max(this.linearVelocity - 1, MinLinearVelocity);
            }

            if (scrolledDown)
            {
                this.linearVelocity = Math.Min(this.linearVelocity + 1, MaxLinearVelocity);
            }
        }
    }
}
