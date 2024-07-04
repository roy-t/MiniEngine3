using System.Diagnostics;
using System.Numerics;
using LibGame.Physics;
using Mini.Engine.Configuration;
using Mini.Engine.Windows;

namespace Mini.Engine.Graphics.Cameras;
[Service]
public sealed class CameraController
{
    private static readonly VirtualKeyCode CodeLeft = VirtualKeyCode.VK_A;
    private static readonly VirtualKeyCode CodeRight = VirtualKeyCode.VK_D;
    private static readonly VirtualKeyCode CodeForward = VirtualKeyCode.VK_W;
    private static readonly VirtualKeyCode CodeBackward = VirtualKeyCode.VK_S;
    private static readonly VirtualKeyCode CodeUp = VirtualKeyCode.VK_SPACE;
    private static readonly VirtualKeyCode CodeDown = VirtualKeyCode.VK_C;
    private static readonly VirtualKeyCode CodeReset = VirtualKeyCode.VK_R;

    private const float MinLinearVelocity = 1.0f;
    private const float MaxLinearVelocity = 25.0f;

    private readonly float AngularVelocity = MathF.PI;

    private float linearVelocity = 5.0f;

    private readonly SimpleKeyboard Keyboard;
    private readonly SimpleMouse Mouse;
    private readonly Stopwatch Stopwatch;

    public CameraController(SimpleInputService inputController)
    {
        this.Keyboard = inputController.Keyboard;
        this.Mouse = inputController.Mouse;

        this.Stopwatch = Stopwatch.StartNew();
    }

    public void Update(ref Transform cameraTransform)
    {
        var elapsed = (float)this.Stopwatch.Elapsed.TotalSeconds;
        this.Stopwatch.Restart();

        var reset = this.Keyboard.Pressed(CodeReset);


        // If a key is held it will not generate new events
        var horizontal = this.Keyboard.AsVector(InputState.Held, CodeForward, CodeLeft, CodeBackward, CodeRight);
        var vertical = this.Keyboard.AsVector(InputState.Held, CodeUp, CodeDown);

        if (reset)
        {
            cameraTransform = Transform.Identity
                .SetTranslation(Vector3.UnitZ * 10)
                .FaceTargetConstrained(Vector3.Zero, Vector3.UnitY);
        }

        if (horizontal.LengthSquared() > 0 || vertical.LengthSquared() > 0)
        {
            var step = elapsed * this.linearVelocity;

            var forward = cameraTransform.GetForward();
            var backward = -forward;
            var up = cameraTransform.GetUp();
            var down = -up;
            var left = cameraTransform.GetLeft();
            var right = -left;

            var translation = Vector3.Zero;
            translation += horizontal.X * forward;
            translation += horizontal.Y * left;
            translation += horizontal.Z * backward;
            translation += horizontal.W * right;

            translation += vertical.X * up;
            translation += vertical.Y * down;

            translation *= step;

            cameraTransform = cameraTransform.AddTranslation(translation);
        }

        var movement = this.Mouse.Movement;
        var scroll = this.Mouse.ScrollDelta;
        var rightButtonDown = this.Mouse.Held(MouseButton.Right);

        if (movement.LengthSquared() > 0 && rightButtonDown)
        {
            movement *= this.AngularVelocity * 0.05f * elapsed;
            var rotation = Quaternion.Identity;
            rotation *= Quaternion.CreateFromAxisAngle(cameraTransform.GetUp(), movement.X);
            rotation *= Quaternion.CreateFromAxisAngle(-cameraTransform.GetLeft(), movement.Y);

            var lookAt = cameraTransform.GetPosition() + Vector3.Transform(cameraTransform.GetForward(), rotation);
            cameraTransform = cameraTransform.FaceTargetConstrained(lookAt, Vector3.UnitY);
        }

        this.linearVelocity = Math.Clamp(this.linearVelocity - scroll, MinLinearVelocity, MaxLinearVelocity);
    }
}
