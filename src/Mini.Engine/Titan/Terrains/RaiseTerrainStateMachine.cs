using System.Drawing;
using System.Numerics;
using LibGame.Mathematics;
using LibGame.Physics;
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

    private bool hasTarget;
    private int targetColumn;
    private int targetRow;
    private TileCorner? targetCorner;
    private Vector2 cursorStartPosition;
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
        if (this.state == State.Target)
        {
            this.hasTarget = false;
            if (viewport.Contains((int)cursor.X, (int)cursor.Y))
            {
                var wvp = camera.GetViewProjection(in transform);
                var (p, d) = Picking.CalculateCursorRay(cursor, in viewport, in wvp);
                var ray = new Ray(p, d);
                if (this.Terrain.Bounds.CheckTileHit(ray, out var tileIndex, out var pit))
                {
                    var tile = this.Terrain.Tiles[tileIndex];
                    var (c, r) = Indexes.ToTwoDimensional(tileIndex, this.Terrain.Columns);

                    var (corner, distance) = GetClosestCorner(tile, c, r, pit);
                    if (distance < 0.15f)
                    {
                        this.targetCorner = corner;
                        this.TargetTransform = CreateCornerTransform(tile, corner, c, r);
                    }
                    else
                    {
                        this.targetCorner = null;
                        this.TargetTransform = CreateWholeTileTransform(tile, c, r);
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

        var diff = MathF.Floor((this.cursorStartPosition.Y - cursor.Y) * 0.05f);
        if (this.state == State.Raise)
        {
            var tile = this.Terrain.Tiles[Indexes.ToOneDimensional(this.targetColumn, this.targetRow, this.Terrain.Columns)];

            if (this.targetCorner != null)
            {
                this.TargetTransform = CreateCornerTransform(tile, this.targetCorner.Value, this.targetColumn, this.targetRow, diff);
            }
            else
            {
                this.TargetTransform = CreateWholeTileTransform(tile, this.targetColumn, this.targetRow, diff);
            }
        }

        if (diff != 0 && this.state == State.Raise && released)
        {
            // Commit changes
            if (this.targetCorner != null)
            {
                this.Terrain.MoveTileCorner(this.targetColumn, this.targetRow, this.targetCorner.Value, (int)diff);
            }
            else
            {
                this.Terrain.MoveTile(this.targetColumn, this.targetRow, (int)diff);
            }

        }

        if (released)
        {
            // Revert to normal state
            this.state = State.Target;
        }
    }


    private static Matrix4x4 CreateCornerTransform(Tile tile, TileCorner corner, int column, int row, float offset = 0.0f)
    {
        var cornerPosition = TileUtilities.GetCornerPosition(column, row, tile, corner);
        var tilePosition = GetTileCenter(tile, column, row);
        var position = Vector3.Lerp(cornerPosition, tilePosition, 0.3f);
        var scale = new Vector3(0.2f, 0.1f, 0.2f);
        return Matrix4x4.CreateScale(scale) * Matrix4x4.CreateTranslation(position + (Vector3.UnitY * offset));
    }

    private static Matrix4x4 CreateWholeTileTransform(Tile tile, int column, int row, float offset = 0.0f)
    {
        var position = GetTileCenter(tile, column, row);
        var scale = new Vector3(0.45f, 0.1f, 0.45f);
        return Matrix4x4.CreateScale(scale) * Matrix4x4.CreateTranslation(position + (Vector3.UnitY * offset));
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
