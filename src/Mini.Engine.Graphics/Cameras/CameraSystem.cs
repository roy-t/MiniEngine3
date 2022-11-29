using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.ECS.Systems;
using Mini.Engine.Windows;
using static Windows.Win32.UI.Input.KeyboardAndMouse.VIRTUAL_KEY;

namespace Mini.Engine.Graphics.Cameras;

[Service]
public sealed partial class CameraSystem : ISystem
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

    private readonly float AngularVelocity = MathF.PI * 0.002f;

    private float linearVelocity = 5.0f;

    private readonly FrameService FrameService;

    private readonly Mouse Mouse;
    private readonly Keyboard Keyboard;
    private readonly InputService InputController;

    public CameraSystem(InputService inputController, FrameService frameService)
    {
        this.Mouse = new Mouse();
        this.Keyboard = new Keyboard();
        this.InputController = inputController;
        this.FrameService = frameService;
    }

    public void OnSet() { }

    [Process]
    public void Update()
    {
        var elapsed = this.FrameService.Elapsed;
        ref var transformComponent = ref this.FrameService.GetPrimaryCameraTransform();        

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
            transformComponent.Current = Transform.Identity
                .SetTranslation(Vector3.UnitZ * 10)
                .FaceTargetConstrained(Vector3.Zero, Vector3.UnitY);
        }

        if (horizontal.LengthSquared() > 0 || vertical.LengthSquared() > 0)
        {
            var step = elapsed * this.linearVelocity;

            var forward = transformComponent.Current.GetForward();
            var backward = -forward;
            var up = transformComponent.Current.GetUp();
            var down = -up;
            var left = transformComponent.Current.GetLeft();
            var right = -left;

            var translation = Vector3.Zero;
            translation += horizontal.X * forward;
            translation += horizontal.Y * left;
            translation += horizontal.Z * backward;
            translation += horizontal.W * right;

            translation += vertical.X * up;
            translation += vertical.Y * down;

            translation *= step;

            transformComponent.Current = transformComponent.Current.AddTranslation(translation);
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
            rotation *= Quaternion.CreateFromAxisAngle(transformComponent.Current.GetUp(), movement.X);
            rotation *= Quaternion.CreateFromAxisAngle(-transformComponent.Current.GetLeft(), movement.Y);

            var lookAt = transformComponent.Current.GetPosition() + Vector3.Transform(transformComponent.Current.GetForward(), rotation);
            transformComponent.Current = transformComponent.Current.FaceTargetConstrained(lookAt, Vector3.UnitY);
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

    public void OnUnSet() { }
}
