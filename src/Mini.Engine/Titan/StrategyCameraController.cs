using System.Drawing;
using System.Numerics;
using LibGame.Mathematics;
using LibGame.Physics;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.Graphics.Cameras;
using Mini.Engine.Windows;
using Vortice.Mathematics;

namespace Mini.Engine.Titan;

[Service]
internal sealed class StrategyCameraController
{
    private static readonly VirtualKeyCode KeyReset = VirtualKeyCode.VK_R;
    private static readonly VirtualKeyCode KeyRotateUp = VirtualKeyCode.VK_OEM_7; // '
    private static readonly VirtualKeyCode KeyRotateDown = VirtualKeyCode.VK_OEM_2; // ?
    private static readonly VirtualKeyCode KeyRotateCW = VirtualKeyCode.VK_OEM_COMMA;
    private static readonly VirtualKeyCode KeyRotateCCW = VirtualKeyCode.VK_OEM_PERIOD;

    // TODO: it could be nice if we can stafe with WASD and rotate with QE, but what keys to use for tilt?

    private const float DistanceMin = 10.0f;
    private const float DistanceMax = 1000.0f;
    private const float ZoomSpeed = 75.0f;
    private const float ClimbSpeed = 1.0f;
    private const float RotateSpeed = MathHelper.TwoPi * 0.5f;

    private readonly SimpleKeyboard Keyboard;
    private readonly SimpleMouse Mouse;

    private Vector3 target;
    private float slope;
    private float rotation;
    private float distance;

    public StrategyCameraController(Device device, SimpleInputService inputService)
    {
        this.Keyboard = inputService.Keyboard;
        this.Mouse = inputService.Mouse;
        this.Camera = CreateCamera(device.Width, device.Height);

        this.ResetParameters();
    }

    public Transform Transform { get; private set; }
    public PerspectiveCamera Camera { get; private set; }

    public void Resize(float width, float height)
    {
        this.Camera = CreateCamera(width, height);
    }

    public void Update(float elapsedRealWorldTime, in Rectangle viewport)
    {
        if (this.Keyboard.Pressed(KeyReset))
        {
            this.ResetParameters();
        }

        // TODO: it would be better if this rotates around the current mouse position
        var rotationAccumulator = 0.0f;
        rotationAccumulator += this.Keyboard.AsFloat(InputState.Held, KeyRotateCW);
        rotationAccumulator -= this.Keyboard.AsFloat(InputState.Held, KeyRotateCCW);
        this.rotation = Radians.WrapRadians(this.rotation + (rotationAccumulator * RotateSpeed * elapsedRealWorldTime));

        var slopeAccumulator = 0.0f;
        slopeAccumulator += this.Keyboard.AsFloat(InputState.Held, KeyRotateUp);
        slopeAccumulator -= this.Keyboard.AsFloat(InputState.Held, KeyRotateDown);
        this.slope = Math.Clamp(this.slope + (slopeAccumulator * elapsedRealWorldTime * ClimbSpeed), 0.1f, 0.9f);

        var scrollAccumulator = (float)this.Mouse.ScrollDelta;
        scrollAccumulator = Math.Clamp(scrollAccumulator, -1.0f, 1.0f);
        var zoomProgress = Ranges.Map(this.distance, (0.0f, DistanceMax), (0.0f, 1.0f));
        this.UpdateTarget(scrollAccumulator * zoomProgress * ZoomSpeed * 0.5f, in viewport);
        this.Transform = this.GetCameraTransform();
    }

    private void ResetParameters()
    {
        this.target = Vector3.Zero;
        this.slope = 0.5f;
        this.rotation = 0.0f;
        this.distance = 350.0f;
    }

    private Transform GetCameraTransform()
    {
        var vector = new Vector3(MathF.Sin(this.rotation), 0.0f, MathF.Cos(this.rotation));

        var vertical = this.distance * this.slope;
        var horizontal = this.distance * (1.0f - this.slope);

        var position = this.target + new Vector3(vector.X * horizontal, vertical, vector.Z * horizontal);

        return Transform.Identity
            .SetTranslation(position)
            .FaceTargetConstrained(this.target, Vector3.UnitY);
    }

    private void UpdateTarget(float distanceChange, in Rectangle viewport)
    {
        if (distanceChange != 0.0f)
        {
            var cursor = this.Mouse.Position;
            if (IsMouseCursorInsideViewport(cursor, in viewport))
            {
                if (distanceChange < 0.0f)
                {
                    var transform = this.GetCameraTransform();
                    var viewProjection = this.Camera.GetViewProjection(transform);
                    var world = GetWorldPositionUnderMouseCursor(cursor, in viewProjection, in viewport, this.target);

                    this.distance = Math.Clamp(this.distance + distanceChange, DistanceMin, DistanceMax);

                    transform = this.GetCameraTransform();
                    viewProjection = this.Camera.GetViewProjection(transform);
                    var newWorld = GetWorldPositionUnderMouseCursor(cursor, in viewProjection, in viewport, this.target);
                    var change = newWorld - world;

                    // TODO: clamp target to the terrain so users can't scroll off the map
                    this.target -= change;
                }
                else
                {
                    this.distance = Math.Clamp(this.distance + distanceChange, DistanceMin, DistanceMax);
                }
            }
        }
    }

    private static Vector3 GetWorldPositionUnderMouseCursor(Vector2 cursor, in Matrix4x4 viewProjection, in Rectangle viewport, Vector3 fallback)
    {
        var (position, direction) = Picking.CalculateCursorRay(cursor, in viewport, viewProjection);
        var ray = new Ray(position, direction);
        var plane = new Plane(Vector3.UnitY, 0.0f);
        var intersection = ray.Intersects(plane);
        if (intersection.HasValue)
        {
            return position + (direction * intersection.Value);
        }

        return fallback;
    }

    private static bool IsMouseCursorInsideViewport(Vector2 cursor, in Rectangle viewport)
    {
        return viewport.Contains((int)cursor.X, (int)cursor.Y);
    }

    private static PerspectiveCamera CreateCamera(float width, float height)
    {
        const float nearPlane = 0.1f;
        const float farPlane = 1000.0f;
        const float fieldOfView = MathF.PI / 2.0f;

        var aspectRatio = width / height;
        return new PerspectiveCamera(nearPlane, farPlane, fieldOfView, aspectRatio);
    }
}
