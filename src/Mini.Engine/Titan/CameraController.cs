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

    public void Update(float elapsedRealWorldTime, in PerspectiveCamera camera, float width, float height)
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
            //var rX = Ranges.Map(cursor.X, (0.0f, width), (-1.0f, 1.0f));
            //var rY = Ranges.Map(cursor.Y, (0.0f, height), (-1.0f, 1.0f));

            //var source = new Vector3(rX, rY, 0.0f);

            var transform = GetCameraTransform();
            var view = Matrix4x4.CreateLookAt(transform.GetPosition(), transform.GetPosition() + transform.GetForward(), transform.GetUp());
            var proj = Matrix4x4.CreatePerspectiveFieldOfView(camera.FieldOfView, camera.AspectRatio, camera.NearPlane, camera.FarPlane);
            //var foo = GetWorldPosition(cursor, width, height, proj, view, transform.GetPosition());
            var ray = CalculateCursorRay(cursor, 0.0f, 0.0f, width, height, camera.NearPlane, camera.FarPlane, proj, view);
            var plane = new Plane(Vector3.UnitY, 0.0f);
            var intersection = ray.Intersects(plane);
            if (intersection.HasValue)
            {
                var p = ray.Position + (ray.Direction * intersection.Value);
                Debug.WriteLine(p);
            }
            else
            {
                Debug.WriteLine("nope");
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

    // CalculateCursorRay Calculates a world space ray starting at the camera's
    // "eye" and pointing in the direction of the cursor. Viewport.Unproject is used
    // to accomplish this. see the accompanying documentation for more explanation
    // of the math behind this function.
    public Ray CalculateCursorRay(Vector2 cursor, float x, float y, float width, float height, float minDepth, float maxDepth, Matrix4x4 projectionMatrix, Matrix4x4 viewMatrix)
    {
        var pointX = Ranges.Map(cursor.X, (0.0f, width), (0.0f, 1.0f)); // TODO [0..1] or [-1..1]?
        var pointY = Ranges.Map(cursor.Y, (0.0f, height), (0.0f, 1.0f));
        var position = new Vector2(cursor.X, cursor.Y);
        // create 2 positions in screenspace using the cursor position. 0 is as
        // close as possible to the camera, 1 is as far away as possible.
        var nearSource = new Vector3(position, 0f);
        var farSource = new Vector3(position, 1f);

        // use Viewport.Unproject to tell what those two screen space positions
        // would be in world space. we'll need the projection matrix and view
        // matrix, which we have saved as member variables. We also need a world
        // matrix, which can just be identity.
        var nearPoint = Unproject(x, y, width, height, minDepth, maxDepth, nearSource,
            projectionMatrix, viewMatrix, Matrix4x4.Identity);

        var farPoint = Unproject(x, y, width, height, minDepth, maxDepth, farSource,
            projectionMatrix, viewMatrix, Matrix4x4.Identity);

        // find the direction vector that goes from the nearPoint to the farPoint
        // and normalize it....
        var direction = Vector3.Normalize(farPoint - nearPoint);

        // and then create a new ray using nearPoint as the source.
        return new Ray(nearPoint, direction);
    }

    public static Vector3 Unproject(float x, float y, float width, float height, float minDepth, float maxDepth, Vector3 source, Matrix4x4 projection, Matrix4x4 view, Matrix4x4 world)
    {
        Matrix4x4.Invert(Matrix4x4.Multiply(Matrix4x4.Multiply(world, view), projection), out var matrix);
        source.X = (((source.X - x) / ((float)width)) * 2f) - 1f;
        source.Y = -((((source.Y - y) / ((float)height)) * 2f) - 1f);
        source.Z = (source.Z - minDepth) / (maxDepth - minDepth);
        var vector = Vector3.Transform(source, matrix);
        var a = (((source.X * matrix.M14) + (source.Y * matrix.M24)) + (source.Z * matrix.M34)) + matrix.M44;
        if (!WithinEpsilon(a, 1f))
        {
            vector.X = vector.X / a;
            vector.Y = vector.Y / a;
            vector.Z = vector.Z / a;
        }
        return vector;

    }

    private static bool WithinEpsilon(float a, float b)
    {
        var num = a - b;
        return ((-1.401298E-45f <= num) && (num <= float.Epsilon));
    }
}
