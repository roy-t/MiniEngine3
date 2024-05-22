using System.Drawing;
using System.Numerics;
using LibGame.Mathematics;
using LibGame.Physics;
using LibGame.Tiles;
using Mini.Engine.Graphics.Cameras;
using Mini.Engine.Windows;
using Vortice.Mathematics;

namespace Mini.Engine.Titan.Terrains;
public sealed class RaiseTerrainStateMachine
{
    public interface ITargetLock
    {
        Vector3 GetPosition();
        float GetDelta(Ray ray);
        Matrix4x4 GetTransform(Ray ray);
        void Commit(Terrain terrain, Ray ray);
    }

    private enum State { Select, Raise, Commit };

    private readonly InputService Input;
    private readonly Terrain Terrain;
    private readonly Mouse Mouse;

    private ITargetLock? TargetLock;
    private State state;

    public RaiseTerrainStateMachine(InputService input, Terrain terrain)
    {
        this.state = State.Select;
        this.Input = input;
        this.Terrain = terrain;
        this.Mouse = new Mouse();
    }

    public Matrix4x4 TargetTransform { get; private set; }

    public void Update(in Rectangle viewport, in PerspectiveCamera camera, in Transform transform)
    {
        this.Input.ProcessAllEvents(this.Mouse);
        var pressed = this.Mouse.Pressed(MouseButtons.Left);
        var held = this.Mouse.Held(MouseButtons.Left);

        if (this.state == State.Select)
        {
            var cursor = this.Input.GetCursorPosition();
            var ray = CreateCursorRay(cursor, in viewport, in camera, in transform);

            this.TargetLock = LockTarget(this.Terrain, viewport, cursor, ray);
            if (this.TargetLock != null)
            {
                this.TargetTransform = this.TargetLock.GetTransform(ray);
            }

            if (pressed && this.TargetLock != null)
            {
                this.state = State.Raise;
            }
        }

        if (this.state == State.Raise)
        {
            if ((pressed || held) && this.TargetLock != null)
            {
                var cursor = this.Input.GetCursorPosition();
                var ray = CreateCursorRay(cursor, in viewport, in camera, in transform);

                this.TargetTransform = this.TargetLock.GetTransform(ray);
            }
            else
            {
                this.state = State.Commit;
            }
        }

        if (this.state == State.Commit)
        {
            if (this.TargetLock != null)
            {
                var cursor = this.Input.GetCursorPosition();
                var ray = CreateCursorRay(cursor, in viewport, in camera, in transform);
                this.TargetLock.Commit(this.Terrain, ray);
            }

            this.state = State.Select;
        }
    }

    private static Ray CreateCursorRay(Vector2 cursor, in Rectangle viewport, in PerspectiveCamera camera, in Transform transform)
    {
        var (p, d) = Picking.CalculateCursorRay(cursor, in viewport, camera.GetViewProjection(in transform));
        return new Ray(p, d);
    }

    private static ITargetLock? LockTarget(Terrain terrain, Rectangle viewport, Vector2 cursor, Ray ray)
    {
        if (viewport.Contains((int)cursor.X, (int)cursor.Y))
        {
            if (terrain.Bounds.CheckTileHit(ray, out var tileIndex, out var dist))
            {
                var pit = ray.Position + (ray.Direction * dist);
                var tile = terrain.Tiles[tileIndex];
                var (c, r) = Indexes.ToTwoDimensional(tileIndex, terrain.Columns);

                var (corner, distance) = TileUtilities.GetClosestCorner(tile, c, r, pit);
                if (distance < 0.15f)
                {
                    return new TileCornerTargetLock(c, r, tile, corner);
                }
                else
                {
                    return new TileTargetLock(c, r, tile);
                }
            }
        }

        return null;
    }

    public bool ShouldDrawTarget()
    {
        return this.TargetLock != null;
    }

    private sealed record class TileCornerTargetLock(int Column, int Row, Tile Tile, TileCorner Corner) : ITargetLock
    {
        public Vector3 GetPosition()
        {
            return TileUtilities.GetCornerPosition(this.Column, this.Row, this.Tile, this.Corner);
        }

        public float GetDelta(Ray ray)
        {
            var cornerPosition = TileUtilities.GetCornerPosition(this.Column, this.Row, this.Tile, this.Corner);
            return Dragging.ComputeDragDeltaY(cornerPosition, ray);
        }

        public Matrix4x4 GetTransform(Ray ray)
        {
            var delta = this.GetDelta(ray);
            var cornerPosition = TileUtilities.GetCornerPosition(this.Column, this.Row, this.Tile, this.Corner);
            var tilePosition = TileUtilities.GetCenterPosition(this.Tile, this.Column, this.Row);
            var position = Vector3.Lerp(cornerPosition, tilePosition, 0.3f);
            var scale = new Vector3(0.2f, 0.1f, 0.2f);

            return Matrix4x4.CreateScale(scale) * Matrix4x4.CreateTranslation(position + (Vector3.UnitY * (int)delta));
        }

        public void Commit(Terrain terrain, Ray ray)
        {
            var delta = this.GetDelta(ray);
            terrain.MoveTileCorner(this.Column, this.Row, this.Corner, (int)delta);
        }
    }

    private sealed record class TileTargetLock(int Column, int Row, Tile Tile) : ITargetLock
    {
        public Vector3 GetPosition()
        {
            return TileUtilities.GetCenterPosition(this.Tile, this.Column, this.Row);
        }

        public float GetDelta(Ray ray)
        {
            var position = TileUtilities.GetCenterPosition(this.Tile, this.Column, this.Row);
            return Dragging.ComputeDragDeltaY(position, ray);
        }

        public Matrix4x4 GetTransform(Ray ray)
        {
            var delta = this.GetDelta(ray);
            var position = TileUtilities.GetCenterPosition(this.Tile, this.Column, this.Row);
            var scale = new Vector3(0.45f, 0.1f, 0.45f);

            return Matrix4x4.CreateScale(scale) * Matrix4x4.CreateTranslation(position + (Vector3.UnitY * (int)delta));
        }

        public void Commit(Terrain terrain, Ray ray)
        {
            var delta = this.GetDelta(ray);
            terrain.MoveTile(this.Column, this.Row, (int)delta);
        }
    }
}
