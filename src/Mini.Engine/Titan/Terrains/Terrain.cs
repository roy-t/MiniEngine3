using System.Runtime.InteropServices;
using LibGame.Mathematics;
using LibGame.Noise;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Contexts;
using Shader = Mini.Engine.Content.Shaders.Generated.TitanTerrain;
using Triangle = Mini.Engine.Content.Shaders.Generated.TitanTerrain.TRIANGLE;

namespace Mini.Engine.Titan.Terrains;

/// <summary>
/// This class creates a tile-map like terrain by:
/// - Generating a heightmap using simplex noise and Fractal Browniam Motion
/// - Generating a tile map from the height map
/// - Optimizing the tile map into zones
/// - Generating and uploading the render data to the GPU
///
/// The idea of this class is that it is easy to completely regenerate the entire map every time
/// one of the tiles changes, using the terrain builder. For this The terrain builder keeps all
/// its working buffers intact, so that frequently rebuilding the terrain doesn't put stress on
/// the GC, or slows down because of many small allocations.
///
/// If the terrain gets very large it will still be computationally expensive to completely regenerate
/// it on any change. Therefor we should create a 'SuperTerrain' class that consists of many smaller
/// regions that can be individually rebuild. While the 'SuperTerrain' class allows us to treat the
/// many regions, as if its just one bigger terrain.
/// </summary>
[Service]
public sealed class Terrain : IDisposable
{
    private const byte MinHeight = 50;
    private const byte CliffStartHeight = 62;
    private const byte CliffLength = 4;
    private const byte MaxHeight = 70;

    private readonly IndexBuffer<int> Indices;
    private readonly VertexBuffer<TerrainVertex> Vertices;
    private readonly StructuredBuffer<Triangle> TrianglesBuffer;
    private readonly ShaderResourceView<Triangle> TrianglesView;

    private readonly Grid<Tile> ModifiableTiles;
    private readonly TerrainBuilder Builder;

    private int simulationVersion;
    private int uploadedVersion;

    public Terrain(Device device, Shader shader)
    {
        this.Columns = 256;
        this.Rows = 256;

        this.uploadedVersion = 0;
        this.simulationVersion = 1;

        var heightMap = GenerateHeightMap(this.Columns, this.Rows);
        this.ModifiableTiles = GetTiles(heightMap, this.Columns);

        this.Builder = new TerrainBuilder(this.Tiles);
        this.Bounds = new TerrainBVH(this.Tiles);

        this.Vertices = new VertexBuffer<TerrainVertex>(device, nameof(Terrain));
        this.Indices = new IndexBuffer<int>(device, nameof(Terrain));
        this.TrianglesBuffer = new StructuredBuffer<Triangle>(device, nameof(Terrain), this.Columns * this.Rows * 2);
        this.TrianglesView = this.TrianglesBuffer.CreateShaderResourceView();
    }

    public int TileIndexOffset { get; private set; }
    public int TileIndexCount { get; private set; }

    public int Columns { get; }
    public int Rows { get; }

    public (VertexBuffer<TerrainVertex> Vertices, IndexBuffer<int> Indices, ShaderResourceView<Triangle> Triangles) GetRenderData(DeviceContext context)
    {
        var simulationVersion = this.simulationVersion;
        var uploadedVersion = this.uploadedVersion;
        if (this.Builder.IsUpToDate(simulationVersion))
        {
            if (simulationVersion > uploadedVersion)
            {
                this.CopyDataToGPU(context);
                this.uploadedVersion = simulationVersion;
            }
        }
        else
        {
            // TODO: this puts a lot of threads in a queue if it takes more than 1 frame to update the builder
            ThreadPool.QueueUserWorkItem(_ => this.Builder.Update(simulationVersion));
        }

        return (this.Vertices, this.Indices, this.TrianglesView);
    }

    public IReadOnlyGrid<Tile> Tiles => this.ModifiableTiles;

    public TerrainBVH Bounds { get; }

    private static byte[] GenerateHeightMap(int columns, int rows)
    {
        var heights = new byte[columns * rows];
        Parallel.For(0, heights.Length, i =>
        {
            var (x, y) = Indexes.ToTwoDimensional(i, columns);
            var noise = FractalBrownianMotion.Generate(SimplexNoise.Noise, x * 0.001f, y * 0.001f, 1.5f, 0.9f, 5);
            noise = Ranges.Map(noise, (-1.0f, 1.0f), (MinHeight, MaxHeight));

            if (noise >= CliffStartHeight)
            {
                noise += CliffLength;
            }

            heights[i] = (byte)noise;
        });

        return heights;
    }

    private static Grid<Tile> GetTiles(byte[] heights, int stride)
    {
        var columns = stride;
        var rows = heights.Length / stride;
        var terrain = new Tile[columns * rows];

        // First tile
        terrain[0] = new Tile(heights[0]);

        // First row
        for (var i = 1; i < columns; i++)
        {
            var leftTile = terrain[i - 1];
            var t = TileUtilities.GetNeighboursFromGrid(heights, columns, rows, i, heights[i]);
            terrain[i] = TileUtilities.FitFirstRow(leftTile, t.E, t.SW, t.S, t.SE, heights[i]);
        }

        // First column
        for (var i = stride; i < heights.Length; i += stride)
        {
            var topTile = terrain[i - stride];
            var t = TileUtilities.GetNeighboursFromGrid(heights, columns, rows, i, heights[i]);
            terrain[i] = TileUtilities.FitFirstColumn(topTile, t.NE, t.E, t.S, t.SE, heights[i]);
        }

        // Fill
        for (var i = 0; i < heights.Length; i++)
        {
            var (x, y) = Indexes.ToTwoDimensional(i, stride);
            if (x > 0 && y > 0)
            {
                var tt = TileUtilities.GetNeighboursFromGrid(terrain, columns, rows, i, terrain[i]);
                var hh = TileUtilities.GetNeighboursFromGrid(heights, columns, rows, i, heights[i]);
                terrain[i] = TileUtilities.Fit(tt.NW, tt.N, tt.NE, tt.W, hh.E, hh.SW, hh.S, hh.SE, heights[i]);
            }
        }

        return new Grid<Tile>(terrain, columns, rows);
    }

    public void Dispose()
    {
        this.Vertices.Dispose();
        this.Indices.Dispose();

        this.TrianglesView.Dispose();
        this.TrianglesBuffer.Dispose();
    }

    public void MoveTile(int column, int row, int diff)
    {
        var original = this.Tiles[column, row];
        var (ne, se, sw, nw) = original.UnpackAll();
        var offset = (byte)Math.Clamp(original.Offset + diff, byte.MinValue, byte.MaxValue);
        this.ModifiableTiles[column, row] = new Tile(ne, se, sw, nw, offset);
        this.Update(column, row);
    }

    public void MoveTileCorner(int targetColumn, int targetRow, TileCorner value, int diff)
    {
        // TODO ALSO: double think about the whole threading mechanism
        throw new Exception("TODO: move a single corner!");
    }

    private void Update(int column, int row)
    {
        this.Bounds.Update(column, row);
        this.simulationVersion++;
    }

    private void CopyDataToGPU(DeviceContext context)
    {
        this.Vertices.MapData(context, CollectionsMarshal.AsSpan(this.Builder.Vertices));
        this.Indices.MapData(context, CollectionsMarshal.AsSpan(this.Builder.Indices));
        this.TrianglesBuffer.MapData(context, CollectionsMarshal.AsSpan(this.Builder.Triangles));

        this.TileIndexOffset = 0;
        this.TileIndexCount = this.Builder.Indices.Count;
    }
}
