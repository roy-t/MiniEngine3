using System.Diagnostics;
using System.Numerics;
using LibGame.Physics;
using Mini.Engine.Configuration;
using Mini.Engine.Windows;

using static Windows.Win32.UI.Input.KeyboardAndMouse.VIRTUAL_KEY;

namespace Mini.Engine.Graphics.Cameras;
[Service]
public sealed class CameraController
{
    private static readonly ushort CodeLeft = InputService.GetScanCode(VK_A);
    private static readonly ushort CodeRight = InputService.GetScanCode(VK_D);
    private static readonly ushort CodeForward = InputService.GetScanCode(VK_W);
    private static readonly ushort CodeBackward = InputService.GetScanCode(VK_S);
    private static readonly ushort CodeUp = InputService.GetScanCode(VK_SPACE);
    private static readonly ushort CodeDown = InputService.GetScanCode(VK_C);
    private static readonly ushort CodeReset = InputService.GetScanCode(VK_R);

    private const float MinLinearVelocity = 1.0f;
    private const float MaxLinearVelocity = 25.0f;

    private readonly float AngularVelocity = MathF.PI;

    private float linearVelocity = 5.0f;

    private readonly Mouse Mouse;
    private readonly Keyboard Keyboard;
    private readonly InputService InputController;
    private readonly Stopwatch Stopwatch;

    public CameraController(InputService inputController)
    {
        this.InputController = inputController;

        this.Mouse = new Mouse();
        this.Keyboard = new Keyboard();
        this.Stopwatch = Stopwatch.StartNew();
    }

    public void Update(ref Transform cameraTransform)
    {
        var elapsed = (float)this.Stopwatch.Elapsed.TotalSeconds;
        this.Stopwatch.Restart();

        var reset = false;
        while (this.InputController.ProcessEvents(this.Keyboard))
        {
            reset |= this.Keyboard.Pressed(CodeReset);
        }

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

        var movement = Vector2.Zero;
        var scroll = 0;
        var rightButtonDown = false;

        while (this.InputController.ProcessEvents(this.Mouse))
        {
            movement += this.Mouse.Movement;
            scroll += this.Mouse.ScrolledUp ? 1 : 0;
            scroll -= this.Mouse.ScrolledDown ? 1 : 0;
            rightButtonDown |= this.Mouse.Held(MouseButtons.Right);
        }

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
