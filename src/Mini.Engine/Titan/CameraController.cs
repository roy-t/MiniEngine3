using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using LibGame.Mathematics;
using LibGame.Physics;
using Mini.Engine.Configuration;
using Mini.Engine.Graphics.Cameras;
using Mini.Engine.Windows;
using Vortice.Mathematics;

namespace Mini.Engine.Titan;

[Service]
internal sealed class CameraController
{
    private const float ZoomSpeed = 1.0f;

    private readonly InputService InputService;
    private readonly Keyboard Keyboard;
    private readonly Mouse Mouse;

    private Vector3 target;
    private float rotation;
    private float distance;

    public CameraController(InputService inputService)
    {
        this.Keyboard = new Keyboard();
        this.Mouse = new Mouse();

        this.InputService = inputService;

        this.target = Vector3.Zero;
        this.rotation = 0.0f;
        this.distance = 10.0f;
    }

    public void Update(float elapsedRealWorldTime, in PerspectiveCamera camera, in Rectangle viewport)
    {
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

        if (zoomAccumulator != 0.0f)
        {
            var cursor = this.InputService.GetCursorPosition();
            var transform = this.GetCameraTransform();
            var wvp = camera.GetViewProjection(in transform);
            var (position, direction) = Picking.CalculateCursorRay(cursor, in viewport, wvp);
            var ray = new Ray(position, direction);
            var plane = new Plane(Vector3.UnitY, 0.0f);
            var intersection = ray.Intersects(plane);
            if (intersection.HasValue)
            {
                
            }


            //var foo = GetWorldPosition2(cursor, width, height, camera.GetViewProjection(in transform), transform.GetPosition());
            //this.target += new Vector3(rX, 0.0f, rY) * -zoomAccumulator;
            this.distance = Math.Clamp(this.distance + zoomAccumulator, 1.0f, 100.0f);            
        }
    }

    public Transform GetCameraTransform()
    {
        var position = this.target + (new Vector3(0.0f, 0.5f, 0.5f) * this.distance);

        return Transform.Identity
            .SetTranslation(position)
            .FaceTarget(this.target);
    }

    public static Vector3 GetWorldPosition2(Vector2 cursor, float width, float height, Matrix4x4 viewProjection, Vector3 cameraPosition)
    {
        var pointX = Ranges.Map(cursor.X, (0.0f, width), (-1.0f, 1.0f));
        var pointY = Ranges.Map(cursor.Y, (0.0f, height), (-1.0f, 1.0f));
        var point = new Vector3(pointX, pointY, 0.0f);
        Matrix4x4.Invert(viewProjection, out var inverseViewProjection);

        return Vector3.Transform(point, inverseViewProjection);
    }

    public static Vector3 GetWorldPosition(Vector2 cursor, float width, float height, Matrix4x4 projection, Matrix4x4 view, Vector3 cameraPosition)
    {
        var pointX = Ranges.Map(cursor.X, (0.0f, width), (-1.0f, 1.0f));
        var pointY = Ranges.Map(cursor.Y, (0.0f, height), (-1.0f, 1.0f));

        pointX = pointX / projection.M11;
        pointY = pointY / projection.M22;

        Matrix4x4.Invert(view, out var inverseViewMatrix);
        
        // Calculate the direction of the picking ray in view space.
        var direction = new Vector3
        (
            (pointX * inverseViewMatrix.M11) + (pointY * inverseViewMatrix.M21) + inverseViewMatrix.M31,
            (pointX * inverseViewMatrix.M12) + (pointY * inverseViewMatrix.M22) + inverseViewMatrix.M32,
            (pointX * inverseViewMatrix.M13) + (pointY * inverseViewMatrix.M23) + inverseViewMatrix.M33
        );

        direction.Y = -direction.Y;
        direction = Vector3.Normalize(direction);
        var origin = cameraPosition;

        var ray = new Ray(in origin, in direction);
        var plane = new Plane(Vector3.UnitY, 0.0f);
        var d = ray.Intersects(plane) ?? 0.0f;        
        return ray.Position + (ray.Direction * d);
    }



   
}
