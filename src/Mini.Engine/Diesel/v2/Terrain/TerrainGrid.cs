using System.Numerics;
using LibGame.Mathematics;
using Mini.Engine.Content;
using Mini.Engine.Diesel.Trains;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;
using Mini.Engine.Graphics.Buffers;
using Mini.Engine.Graphics.Primitives;

namespace Mini.Engine.Diesel.v2.Terrain;

public enum Orientation : byte
{
    North = 0,
    East = 1,
    South = 2,
    West = 3
}

public readonly record struct TerrainCell(byte Height, Entity Type, Orientation Orientation);

public abstract class InstanceGrid<TCellType>
    where TCellType : unmanaged, IEquatable<TCellType>
{
    protected readonly IComponentContainer<InstancesComponent> Instances;
    protected readonly TCellType[] Cells;

    public InstanceGrid(IComponentContainer<InstancesComponent> Instances, int gridWidth, int gridHeight, float cellWidth, float cellHeight)
    {
        this.GridWidth = gridWidth;
        this.GridHeight = gridHeight;
        this.CellWidth = cellWidth;
        this.CellHeight = cellHeight;

        this.Cells = new TCellType[gridWidth * gridHeight];

        this.Instances = Instances;
    }

    public int GridWidth { get; }
    public int GridHeight { get; }
    public float CellWidth { get; }
    public float CellHeight { get; }    

    public void SetCell(int x, int y, in TCellType cell)
    {
        var index = this.ToIndex(x, y);
        var previous = this.Cells[index];

        if (!previous.Equals(cell))
        {
            this.Cells[index] = cell;

            ref var previousInstance = ref this.Instances[this.ToEntity(in previous)];
            previousInstance.Value.InstanceList.Remove(this.ToMatrix(x, y, in previous));
            previousInstance.LifeCycle = previousInstance.LifeCycle.ToChanged();

            ref var nextInstance = ref this.Instances[this.ToEntity(in cell)];
            nextInstance.Value.InstanceList.Add(this.ToMatrix(x, y, in cell));
            nextInstance.LifeCycle = nextInstance.LifeCycle.ToChanged();
        }
    }

    protected void InitializeCell(int x, int y, in TCellType cell)
    {
        this.Cells[this.ToIndex(x, y)] = cell;

        ref var nextInstance = ref this.Instances[this.ToEntity(in cell)];
        nextInstance.Value.InstanceList.Add(this.ToMatrix(x, y, in cell));
        nextInstance.LifeCycle = nextInstance.LifeCycle.ToChanged();
    }

    protected abstract Entity ToEntity(in TCellType cell);
    protected abstract Matrix4x4 ToMatrix(int x, int y, in TCellType cell);

    protected int ToIndex(int x, int y)
    {
        return Indexes.ToOneDimensional(x, y, this.GridWidth);
    }
}

public sealed class TerrainGrid : InstanceGrid<TerrainCell>
{
    public TerrainGrid(IComponentContainer<InstancesComponent> instances, Entity initialEntity, int gridWidth, int gridHeight, float cellWidth, float cellHeight)
        : base(instances, gridWidth, gridHeight, cellWidth, cellHeight)
    {
      
        var cell = new TerrainCell(1, initialEntity, Orientation.North);
        for (var y = 0; y < gridHeight; y++)
        {
            for (var x = 0; x < gridWidth; x++)
            {
                this.InitializeCell(x, y, in cell);
            }
        }
    }

    public byte GetHeight(int x, int y)
    {
        return this.Cells[this.ToIndex(x, y)].Height;
    }

    public Entity GetType(int x, int y)
    {
        return this.Cells[this.ToIndex(x, y)].Type;
    }

    public Orientation GetOrientation(int x, int y)
    {
        return this.Cells[this.ToIndex(x, y)].Orientation;
    }

    protected override Matrix4x4 ToMatrix(int x, int y, in TerrainCell cell)
    {
        const float QuarterTurn = MathF.PI * -0.5f;
        var rotation = (float)cell.Orientation * QuarterTurn;
        var translation = new Vector3(x * this.CellWidth, cell.Height * this.CellHeight, y * this.CellWidth);

        return Matrix4x4.CreateRotationY(rotation) * Matrix4x4.CreateTranslation(translation);
    }

    protected override Entity ToEntity(in TerrainCell cell)
    {
        return cell.Type;
    }

    private static UploadBuffer<Matrix4x4>[] CreateUploadBuffers(Device device, int gridWidth, int gridHeight)
    {
        return new UploadBuffer<Matrix4x4>[]
        {
            new UploadBuffer<Matrix4x4>(device, "Flat", gridWidth * gridHeight),
            new UploadBuffer<Matrix4x4>(device, "Slope", Math.Min(100, gridWidth * gridHeight)),
            new UploadBuffer<Matrix4x4>(device, "Cliff", Math.Min(100, gridWidth * gridHeight))
        };
    }
}