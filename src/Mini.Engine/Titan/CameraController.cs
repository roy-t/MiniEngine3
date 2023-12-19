using System.Drawing;
using System.Numerics;
using LibGame.Mathematics;
using LibGame.Physics;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.Graphics.Cameras;
using Mini.Engine.Windows;
using Vortice.Mathematics;

using static Windows.Win32.UI.Input.KeyboardAndMouse.VIRTUAL_KEY;

namespace Mini.Engine.Titan;


[Service]
internal sealed class CameraController
{
    private static readonly ushort KeyRotateUp = InputService.GetScanCode(VK_A);
    private static readonly ushort KeyRotateDown = InputService.GetScanCode(VK_Z);
    private static readonly ushort KeyRotateCW = InputService.GetScanCode(VK_OEM_COMMA);
    private static readonly ushort KeyRotateCCW = InputService.GetScanCode(VK_OEM_PERIOD);

    private const float ZoomSpeed = 1.0f;
    private const float MoveSpeed = 0.1f;
    private const float ClimbSpeed = 1.0f;
    private const float RotateSpeed = MathHelper.TwoPi;

    private readonly InputService InputService;
    private readonly Keyboard Keyboard;
    private readonly Mouse Mouse;

    private float climb;
    private Vector3 target;
    private float rotation;
    private float distance;

    public CameraController(Device device, InputService inputService)
    {
        this.Keyboard = new Keyboard();
        this.Mouse = new Mouse();
        this.InputService = inputService;

        this.target = Vector3.Zero;
        this.climb = 0.5f;
        this.rotation = 0.0f;
        this.distance = 10.0f;

        this.Camera = CreateCamera(device.Width, device.Height);
    }

    public Transform Transform { get; private set; }
    public PerspectiveCamera Camera { get; private set; }

    public void Resize(float width, float height)
    {
        this.Camera = CreateCamera(width, height);
    }

    public void Update(float elapsedRealWorldTime, in Rectangle viewport)
    {
        var rotationAccumulator = 0.0f;
        var climbAccumulator = 0.0f;
        while (this.InputService.ProcessEvents(this.Keyboard))
        {
            if (this.Keyboard.Pressed(KeyRotateCW) || this.Keyboard.Held(KeyRotateCW))
            {
                rotationAccumulator += 1.0f;
            }

            if (this.Keyboard.Pressed(KeyRotateCCW) || this.Keyboard.Held(KeyRotateCCW))
            {
                rotationAccumulator -= 1.0f;
            }

            if (this.Keyboard.Pressed(KeyRotateUp) || this.Keyboard.Held(KeyRotateUp))
            {
                climbAccumulator += 1.0f;
            }            

            if (this.Keyboard.Pressed(KeyRotateDown) || this.Keyboard.Held(KeyRotateDown))
            {
                climbAccumulator -= 1.0f;
            }
        }

        this.rotation = Radians.WrapRadians(this.rotation + (rotationAccumulator * RotateSpeed * elapsedRealWorldTime));
        this.climb = Math.Clamp(this.climb + (climbAccumulator * elapsedRealWorldTime * ClimbSpeed), 0.1f, 0.9f);

        var zoomAccumulator = 0.0f;
        while (this.InputService.ProcessEvents(this.Mouse))
        {
            if (this.Mouse.ScrolledDown)
            {
                zoomAccumulator -= ZoomSpeed;
            }

            if (this.Mouse.ScrolledUp)
            {
                zoomAccumulator += ZoomSpeed;
            }
        }

        var newTarget = this.GetZoomMovementTarget(zoomAccumulator, in viewport);
        this.target = Vector3.Lerp(this.target, newTarget, MoveSpeed);
        this.distance = Math.Clamp(this.distance + zoomAccumulator, 1.0f, 100.0f);

        this.Transform = this.GetCameraTransform();
    }

    private Vector3 GetZoomMovementTarget(float zoomChange, in Rectangle viewport)
    {
        if (zoomChange < 0.0f)
        {
            var cursor = this.InputService.GetCursorPosition();
            var (position, direction) = Picking.CalculateCursorRay(cursor, in viewport, this.Camera.GetViewProjection(this.Transform));
            var ray = new Ray(position, direction);
            var plane = new Plane(Vector3.UnitY, 0.0f);
            var intersection = ray.Intersects(plane);
            if (intersection.HasValue)
            {                
                return position + (direction * intersection.Value);
            }
        }

        return this.target;
    }

    private Transform GetCameraTransform()
    {
        var bx = Matrix4x4.CreateRotationY(this.rotation);
        var vector = Vector3.TransformNormal(-Vector3.UnitZ, bx);

        var h = this.distance * this.climb;
        var w = this.distance * (1.0f - this.climb);

        var position = this.target + new Vector3(vector.X * w, h, vector.Z * w);
       
        return Transform.Identity
            .SetTranslation(position)
            .FaceTargetConstrained(this.target, Vector3.UnitY);
    }

    private static PerspectiveCamera CreateCamera(float width, float height)
    {
        const float nearPlane = 0.1f;
        const float farPlane = 100.0f;
        const float fieldOfView = MathF.PI / 2.0f;

        var aspectRatio = width / height;
        return new PerspectiveCamera(nearPlane, farPlane, fieldOfView, aspectRatio);
    }
}
