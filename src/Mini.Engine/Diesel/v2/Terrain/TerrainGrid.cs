using System.Numerics;
using LibGame.Mathematics;
using Mini.Engine.ECS;
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
    protected readonly InstancesSystem Instances;
    protected readonly TCellType[] Cells;
    private readonly Dictionary<Entity, List<Matrix4x4>> InstanceLookUp;
    private readonly HashSet<Entity> DirtySet;

    public InstanceGrid(InstancesSystem Instances, int gridWidth, int gridHeight, float cellWidth, float cellHeight)
    {
        this.Instances = Instances;

        this.GridWidth = gridWidth;
        this.GridHeight = gridHeight;
        this.CellWidth = cellWidth;
        this.CellHeight = cellHeight;
                
        this.Cells = new TCellType[gridWidth * gridHeight];
        this.InstanceLookUp = new Dictionary<Entity, List<Matrix4x4>>();
        this.DirtySet = new HashSet<Entity>();
    }

    public IReadOnlyDictionary<Entity, List<Matrix4x4>> LookUp => this.InstanceLookUp;
    public IReadOnlySet<Entity> Dirty => this.DirtySet;
    public int GridWidth { get; }
    public int GridHeight { get; }
    public float CellWidth { get; }
    public float CellHeight { get; }
    public bool Changed { get; private set; }

    public void SetCell(int x, int y, in TCellType cell)
    {
        var index = this.ToIndex(x, y);
        var previous = this.Cells[index];
       
        if (!previous.Equals(cell))
        {
            var entity = this.ToEntity(cell);
            if (!this.InstanceLookUp.TryGetValue(entity, out var list))
            {
                list = new List<Matrix4x4>();
                this.InstanceLookUp.Add(entity, list);
            }

            this.Cells[index] = cell;
            list.Remove(this.ToMatrix(x, y, in previous));
            list.Add(this.ToMatrix(x, y, in cell));
            this.DirtySet.Add(entity);
        }
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
    public TerrainGrid(InstancesSystem instances, Entity initialEntity, int gridWidth, int gridHeight, float cellWidth, float cellHeight)
        : base(instances, gridWidth, gridHeight, cellWidth, cellHeight)
    {      
        var cell = new TerrainCell(1, initialEntity, Orientation.North);
        for (var y = 0; y < gridHeight; y++)
        {
            for (var x = 0; x < gridWidth; x++)
            {
                this.SetCell(x, y, in cell);
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
}