using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Graphics;
using Mini.Engine.Graphics.Cameras;
using Mini.Engine.Graphics.Transforms;
using Mini.Engine.Windows;
using Windows.Win32.UI.KeyboardAndMouseInput;

namespace Mini.Engine.Controllers;

[Service]
public sealed class CameraController
{
    private static readonly ushort CodeLeft = InputService.GetScanCode(VIRTUAL_KEY.VK_A);
    private static readonly ushort CodeRight = InputService.GetScanCode(VIRTUAL_KEY.VK_D);
    private static readonly ushort CodeForward = InputService.GetScanCode(VIRTUAL_KEY.VK_W);
    private static readonly ushort CodeBackward = InputService.GetScanCode(VIRTUAL_KEY.VK_S);
    private static readonly ushort CodeUp = InputService.GetScanCode(VIRTUAL_KEY.VK_SPACE);
    private static readonly ushort CodeDown = InputService.GetScanCode(VIRTUAL_KEY.VK_C);
    private static readonly ushort CodeReset = InputService.GetScanCode(VIRTUAL_KEY.VK_R);

    private const float MinLinearVelocity = 1.0f;
    private const float MaxLinearVelocity = 25.0f;

    private readonly float AngularVelocity = MathF.PI * 0.002f;

    private float linearVelocity = 5.0f;

    private readonly Mouse Mouse;
    private readonly Keyboard Keyboard;
    private readonly InputService InputController;

    public CameraController(InputService inputController)
    {
        this.Mouse = new Mouse();
        this.Keyboard = new Keyboard();
        this.InputController = inputController;
    }

    public void Update(ref CameraComponent cameraComponent, ref TransformComponent transformComponent, float elapsed)
    {
        var horizontal = Vector4.Zero;
        var vertical = Vector2.Zero;
        var reset = false;
        while (this.InputController.ProcessEvents(this.Keyboard))
        {
            horizontal += this.Keyboard.AsVector(InputState.Pressed, CodeForward, CodeLeft, CodeBackward, CodeRight);
            vertical += this.Keyboard.AsVector(InputState.Pressed, CodeUp, CodeDown);
            reset |= this.Keyboard.Pressed(CodeReset);
        }

        if (reset)
        {
            transformComponent.Transform = Transform.Identity
                .SetTranslation(Vector3.UnitZ * 10)
                .FaceTargetConstrained(Vector3.Zero, Vector3.UnitY);
        }

        if (horizontal.LengthSquared() > 0 || vertical.LengthSquared() > 0)
        {
            var step = elapsed * this.linearVelocity;

            var forward = transformComponent.Transform.GetForward();
            var backward = -forward;
            var up = transformComponent.Transform.GetUp();
            var down = -up;
            var left = transformComponent.Transform.GetLeft();
            var right = -left;

            var translation = Vector3.Zero;
            translation += horizontal.X * forward;
            translation += horizontal.Y * left;
            translation += horizontal.Z * backward;
            translation += horizontal.W * right;

            translation += vertical.X * up;
            translation += vertical.Y * down;

            translation *= step;

            transformComponent.Transform = transformComponent.Transform.AddTranslation(translation);
        }

        var movement = Vector2.Zero;
        var scrolledUp = false;
        var scrolledDown = false;
        var rightButtonDown = false;

        while (this.InputController.ProcessEvents(this.Mouse))
        {
            movement += this.Mouse.Movement;
            scrolledUp |= this.Mouse.ScrolledUp;
            scrolledDown |= this.Mouse.ScrolledDown;
            rightButtonDown |= this.Mouse.Held(MouseButtons.Right);
        }

        if (movement.LengthSquared() > 0 && rightButtonDown)
        {
            movement *= this.AngularVelocity;
            var rotation = Quaternion.Identity;
            rotation *= Quaternion.CreateFromAxisAngle(transformComponent.Transform.GetUp(), movement.X);
            rotation *= Quaternion.CreateFromAxisAngle(-transformComponent.Transform.GetLeft(), movement.Y);

            var lookAt = transformComponent.Transform.GetPosition() + Vector3.Transform(transformComponent.Transform.GetForward(), rotation);
            transformComponent.Transform = transformComponent.Transform.FaceTargetConstrained(lookAt, Vector3.UnitY);
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
