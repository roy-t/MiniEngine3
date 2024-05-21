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
    private enum State { Target, Raise, Release };

    private readonly InputService Input;
    private readonly Terrain Terrain;
    private readonly Mouse Mouse;

    private Vector2 cursorStartPosition;
    private TileCorner? targetCorner;
    private bool hasTarget;
    private int targetColumn;
    private int targetRow;
    private float delta;

    private State state;


    public RaiseTerrainStateMachine(InputService input, Terrain terrain)
    {
        this.state = State.Target; // Inactive
        this.Input = input;
        this.Terrain = terrain;
        this.Mouse = new Mouse();
    }

    public Matrix4x4 TargetTransform { get; private set; }

    public void Update(in Rectangle viewport, in PerspectiveCamera camera, in Transform transform)
    {
        var cursor = this.Input.GetCursorPosition();
        var (p, d) = Picking.CalculateCursorRay(cursor, in viewport, camera.GetViewProjection(in transform));
        var ray = new Ray(p, d);

        if (this.state == State.Target)
        {
            this.hasTarget = false;
            if (viewport.Contains((int)cursor.X, (int)cursor.Y))
            {
                if (this.Terrain.Bounds.CheckTileHit(ray, out var tileIndex, out var dist))
                {
                    var pit = ray.Position + (ray.Direction * dist);
                    var tile = this.Terrain.Tiles[tileIndex];
                    var (c, r) = Indexes.ToTwoDimensional(tileIndex, this.Terrain.Columns);

                    var (corner, distance) = GetClosestCorner(tile, c, r, pit);
                    if (distance < 0.15f)
                    {
                        this.targetCorner = corner;
                        this.TargetTransform = CreateCornerTransform(tile, corner, c, r, 0.0f);
                    }
                    else
                    {
                        this.targetCorner = null;
                        this.TargetTransform = CreateWholeTileTransform(tile, c, r, 0.0f);
                    }

                    this.hasTarget = true;
                    this.targetColumn = c;
                    this.targetRow = r;
                }
            }
        }

        var pressed = false;
        var held = false;
        var released = false;
        while (this.Input.ProcessEvents(this.Mouse))
        {
            pressed |= this.Mouse.Pressed(MouseButtons.Left);
            held |= this.Mouse.Held(MouseButtons.Left);
            released |= this.Mouse.Released(MouseButtons.Left);
        }

        if (this.state == State.Target && this.hasTarget && (pressed || held))
        {
            this.cursorStartPosition = cursor;
            this.state = State.Raise;
        }


        if (this.state == State.Raise)
        {
            var tile = this.Terrain.Tiles[Indexes.ToOneDimensional(this.targetColumn, this.targetRow, this.Terrain.Columns)];

            if (this.targetCorner != null)
            {
                this.delta = GetCornerDelta(tile, this.targetCorner.Value, this.targetColumn, this.targetRow, ray);
                this.TargetTransform = CreateCornerTransform(tile, this.targetCorner.Value, this.targetColumn, this.targetRow, this.delta);
            }
            else
            {
                this.delta = GetWholeTileDelta(tile, this.targetColumn, this.targetRow, ray);
                this.TargetTransform = CreateWholeTileTransform(tile, this.targetColumn, this.targetRow, this.delta);
            }
        }

        if (Math.Abs(this.delta) >= 1.0f && this.state == State.Raise && released)
        {
            // Commit changes
            if (this.targetCorner != null)
            {
                this.Terrain.MoveTileCorner(this.targetColumn, this.targetRow, this.targetCorner.Value, (int)this.delta);
            }
            else
            {
                this.Terrain.MoveTile(this.targetColumn, this.targetRow, (int)this.delta);
            }

        }

        if (released)
        {
            // Revert to normal state
            this.state = State.Target;
        }
    }


    private static float GetCornerDelta(Tile tile, TileCorner corner, int column, int row, Ray ray)
    {
        var cornerPosition = TileUtilities.GetCornerPosition(column, row, tile, corner);
        return Dragging.ComputeDragDeltaY(cornerPosition, ray);
    }

    private static float GetWholeTileDelta(Tile tile, int column, int row, Ray ray)
    {
        var position = GetTileCenter(tile, column, row);
        return Dragging.ComputeDragDeltaY(position, ray);
    }


    private static Matrix4x4 CreateCornerTransform(Tile tile, TileCorner corner, int column, int row, float delta)
    {
        var cornerPosition = TileUtilities.GetCornerPosition(column, row, tile, corner);
        var tilePosition = GetTileCenter(tile, column, row);
        var position = Vector3.Lerp(cornerPosition, tilePosition, 0.3f);
        var scale = new Vector3(0.2f, 0.1f, 0.2f);

        return Matrix4x4.CreateScale(scale) * Matrix4x4.CreateTranslation(position + (Vector3.UnitY * (int)delta));
    }

    private static Matrix4x4 CreateWholeTileTransform(Tile tile, int column, int row, float delta)
    {
        var position = GetTileCenter(tile, column, row);
        var scale = new Vector3(0.45f, 0.1f, 0.45f);

        return Matrix4x4.CreateScale(scale) * Matrix4x4.CreateTranslation(position + (Vector3.UnitY * (int)delta));
    }

    public bool ShouldDrawTarget()
    {
        return this.state == State.Target || this.state == State.Raise;
    }


    private static Vector3 GetTileCenter(Tile tile, int column, int row)
    {
        var sum = TileUtilities.GetCornerPosition(column, row, tile, TileCorner.NE) +
                  TileUtilities.GetCornerPosition(column, row, tile, TileCorner.SE) +
                  TileUtilities.GetCornerPosition(column, row, tile, TileCorner.SW) +
                  TileUtilities.GetCornerPosition(column, row, tile, TileCorner.NW);

        return sum / 4.0f;
    }

    private static (TileCorner corner, float distance) GetClosestCorner(Tile tile, int column, int row, Vector3 hit)
    {
        TileCorner[] corners = [TileCorner.NE, TileCorner.SE, TileCorner.SW, TileCorner.NW];
        var bestDistance = float.MaxValue;
        var bestCorner = TileCorner.NE;
        for (var i = 0; i < corners.Length; i++)
        {
            var corner = corners[i];
            var position = TileUtilities.GetCornerPosition(column, row, tile, corner);
            var distance = Vector3.DistanceSquared(position, hit);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestCorner = corner;
            }
        }

        return (bestCorner, bestDistance);
    }
}
