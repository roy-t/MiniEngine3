﻿using System.Runtime.InteropServices;
using LibGame.Mathematics;
using LibGame.Noise;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
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

    private readonly TerrainBuilder Builder;

    public Terrain(Device device, Shader shader)
    {
        this.Columns = 256;
        this.Rows = 256;
        this.Builder = new TerrainBuilder(this.Columns, this.Rows);

        var heightMap = GenerateHeightMap(this.Columns, this.Rows);
        this.Tiles = GetTiles(heightMap, this.Columns);
        this.Bounds = new TerrainBVH(this.Tiles, this.Columns, this.Rows);

        this.Builder.Update(this.Tiles);

        this.TileIndexOffset = 0;
        this.TileIndexCount = this.Builder.Indices.Count;

        this.Vertices = new VertexBuffer<TerrainVertex>(device, nameof(Terrain));
        this.Vertices.MapData(device.ImmediateContext, CollectionsMarshal.AsSpan(this.Builder.Vertices));

        this.Indices = new IndexBuffer<int>(device, nameof(Terrain));
        this.Indices.MapData(device.ImmediateContext, CollectionsMarshal.AsSpan(this.Builder.Indices));

        this.TrianglesBuffer = new StructuredBuffer<Triangle>(device, nameof(Terrain), this.Builder.Triangles.Count);
        this.TrianglesBuffer.MapData(device.ImmediateContext, CollectionsMarshal.AsSpan(this.Builder.Triangles));
        this.TrianglesView = this.TrianglesBuffer.CreateShaderResourceView();
    }

    public int TileIndexOffset { get; }
    public int TileIndexCount { get; }

    public int Columns { get; }
    public int Rows { get; }

    public IndexBuffer<int> Indices { get; }
    public VertexBuffer<TerrainVertex> Vertices { get; }
    public StructuredBuffer<Triangle> TrianglesBuffer { get; }
    public ShaderResourceView<Triangle> TrianglesView { get; }

    public IReadOnlyList<Tile> Tiles { get; }

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

    private static Tile[] GetTiles(byte[] heights, int stride)
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

        return terrain;
    }

    public void Dispose()
    {
        this.Vertices.Dispose();
        this.Indices.Dispose();

        this.TrianglesView.Dispose();
        this.TrianglesBuffer.Dispose();
    }
}
