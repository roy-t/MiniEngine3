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
    private enum State { Select, Raise, Commit };

    private readonly InputService Input;
    private readonly Terrain Terrain;
    private readonly Mouse Mouse;
    private readonly TileTarget Target;

    private State state;

    public RaiseTerrainStateMachine(InputService input, Terrain terrain)
    {
        this.state = State.Select;
        this.Input = input;
        this.Terrain = terrain;
        this.Mouse = new Mouse();
        this.Target = new TileTarget();
    }

    public Matrix4x4 TargetTransform { get; private set; }

    public void Update(in Rectangle viewport, in PerspectiveCamera camera, in Transform transform)
    {
        this.Input.ProcessAllEvents(this.Mouse);
        var pressed = this.Mouse.Pressed(MouseButton.Left);
        var held = this.Mouse.Held(MouseButton.Left);

        if (this.state == State.Select)
        {
            var cursor = this.Input.GetCursorPosition();
            var ray = CreateCursorRay(cursor, in viewport, in camera, in transform);

            if (LockTarget(this.Terrain, viewport, cursor, ray, this.Target))
            {
                this.TargetTransform = this.Target.GetTransform(ray);
            }

            if (pressed && this.Target.HasTarget)
            {
                this.state = State.Raise;
            }
        }

        if (this.state == State.Raise)
        {
            if ((pressed || held) && this.Target.HasTarget)
            {
                var cursor = this.Input.GetCursorPosition();
                var ray = CreateCursorRay(cursor, in viewport, in camera, in transform);

                this.TargetTransform = this.Target.GetTransform(ray);
            }
            else
            {
                this.state = State.Commit;
            }
        }

        if (this.state == State.Commit)
        {
            if (this.Target.HasTarget)
            {
                var cursor = this.Input.GetCursorPosition();
                var ray = CreateCursorRay(cursor, in viewport, in camera, in transform);
                this.Target.Commit(this.Terrain, ray);
            }

            this.state = State.Select;
        }
    }

    private static Ray CreateCursorRay(Vector2 cursor, in Rectangle viewport, in PerspectiveCamera camera, in Transform transform)
    {
        var (p, d) = Picking.CalculateCursorRay(cursor, in viewport, camera.GetViewProjection(in transform));
        return new Ray(p, d);
    }

    private static bool LockTarget(Terrain terrain, Rectangle viewport, Vector2 cursor, Ray ray, TileTarget target)
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
                    target.LockTileCorner(c, r, tile, corner);
                }
                else
                {
                    target.LockTileCenter(c, r, tile);
                }

                return true;
            }
        }

        target.UnLock();

        return false;
    }

    public bool ShouldDrawTarget()
    {
        return this.Target.HasTarget;
    }

    private sealed class TileTarget
    {
        private enum LockType { Unlocked, Center, Corner }

        private int column;
        private int row;
        private Tile tile;
        private TileCorner corner;
        private LockType lockType;

        public TileTarget()
        {
            this.column = 0;
            this.row = 0;
            this.tile = default;
            this.corner = default;
            this.lockType = LockType.Unlocked;
        }

        public bool HasTarget => this.lockType != LockType.Unlocked;

        public void UnLock()
        {
            this.lockType = LockType.Unlocked;
        }

        public void LockTileCenter(int column, int row, Tile tile)
        {
            this.column = column;
            this.row = row;
            this.tile = tile;
            this.lockType = LockType.Center;
        }

        public void LockTileCorner(int column, int row, Tile tile, TileCorner corner)
        {
            this.column = column;
            this.row = row;
            this.tile = tile;
            this.corner = corner;
            this.lockType = LockType.Corner;
        }

        public Vector3 GetPosition()
        {
            return this.lockType switch
            {
                LockType.Center => TileUtilities.GetCenterPosition(this.tile, this.column, this.row),
                LockType.Corner => TileUtilities.GetCornerPosition(this.column, this.row, this.tile, this.corner),
                _ => throw new ArgumentOutOfRangeException(nameof(this.lockType)),
            };
        }

        public float GetDelta(Ray ray)
        {
            return this.lockType switch
            {
                LockType.Center => Dragging.ComputeDragDeltaY(TileUtilities.GetCenterPosition(this.tile, this.column, this.row), ray),
                LockType.Corner => Dragging.ComputeDragDeltaY(TileUtilities.GetCornerPosition(this.column, this.row, this.tile, this.corner), ray),
                _ => throw new ArgumentOutOfRangeException(nameof(this.lockType)),
            };
        }

        public Matrix4x4 GetTransform(Ray ray)
        {
            return this.lockType switch
            {
                LockType.Center => this.GetWholeTileTransform(ray),
                LockType.Corner => this.GetCornerTransform(ray),
                _ => throw new ArgumentOutOfRangeException(nameof(this.lockType)),
            };
        }

        public void Commit(Terrain terrain, Ray ray)
        {
            var delta = this.GetDelta(ray);
            switch (this.lockType)
            {
                case LockType.Center:
                    terrain.MoveTile(this.column, this.row, (int)delta);
                    break;
                case LockType.Corner:
                    terrain.MoveTileCorner(this.column, this.row, this.corner, (int)delta);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(this.lockType));
            };
        }

        private Matrix4x4 GetWholeTileTransform(Ray ray)
        {
            var delta = this.GetDelta(ray);
            var position = TileUtilities.GetCenterPosition(this.tile, this.column, this.row);
            var scale = new Vector3(0.45f, 0.1f, 0.45f);

            return Matrix4x4.CreateScale(scale) * Matrix4x4.CreateTranslation(position + (Vector3.UnitY * (int)delta));
        }

        private Matrix4x4 GetCornerTransform(Ray ray)
        {
            var delta = this.GetDelta(ray);
            var cornerPosition = TileUtilities.GetCornerPosition(this.column, this.row, this.tile, this.corner);
            var tilePosition = TileUtilities.GetCenterPosition(this.tile, this.column, this.row);
            var position = Vector3.Lerp(cornerPosition, tilePosition, 0.3f);
            var scale = new Vector3(0.2f, 0.1f, 0.2f);

            return Matrix4x4.CreateScale(scale) * Matrix4x4.CreateTranslation(position + (Vector3.UnitY * (int)delta));
        }
    }
}
