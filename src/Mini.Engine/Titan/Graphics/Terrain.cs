using System.Numerics;
using System.Runtime.InteropServices;
using LibGame.Graphics;
using LibGame.Mathematics;
using LibGame.Noise;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Shader = Mini.Engine.Content.Shaders.Generated.TitanTerrain;
using Triangle = Mini.Engine.Content.Shaders.Generated.TitanTerrain.TRIANGLE;

namespace Mini.Engine.Titan.Graphics;

[Service]
internal sealed class Terrain : ITerrain, IDisposable
{
    private const int MaxHeight = 10;

    public Terrain(Device device, Shader shader)
    {
        const int columns = 200;
        const int rows = 200;
        var heightMap = GenerateHeightMap(columns, rows);
        for (var i = 0; i < heightMap.Length; i++)
        {
            if (heightMap[i] > 5)
            {
                heightMap[i] = 7;
            }
        }

        var tiles = GetTiles(heightMap, columns);
        var vertices = GetVertices(tiles, columns);
        var indices = GetIndices(tiles);
        var triangles = GetTriangles(tiles, vertices, indices);
        AddCliffs(tiles, indices, triangles, columns, rows);

        this.TileIndexOffset = 0;
        this.TileIndexCount = indices.Count;
        this.GridIndexOffset = this.TileIndexCount;
        this.GridIndexCount = AddGrid(tiles, indices);

        this.Vertices = new VertexBuffer<TerrainVertex>(device, nameof(Terrain));
        this.Vertices.MapData(device.ImmediateContext, CollectionsMarshal.AsSpan(vertices));

        this.Indices = new IndexBuffer<int>(device, nameof(Terrain));
        this.Indices.MapData(device.ImmediateContext, CollectionsMarshal.AsSpan(indices));

        this.TrianglesBuffer = new StructuredBuffer<Triangle>(device, nameof(Terrain), triangles.Count);
        this.TrianglesBuffer.MapData(device.ImmediateContext, CollectionsMarshal.AsSpan(triangles));
        this.TrianglesView = this.TrianglesBuffer.CreateShaderResourceView();
    }

    public int TileIndexOffset { get; }
    public int TileIndexCount { get; }
    public int GridIndexOffset { get; }
    public int GridIndexCount { get; }

    public IndexBuffer<int> Indices { get; }
    public VertexBuffer<TerrainVertex> Vertices { get; }
    public StructuredBuffer<Triangle> TrianglesBuffer { get; }
    public ShaderResourceView<Triangle> TrianglesView { get; }

    private static float[] GenerateHeightMap(int columns, int rows)
    {
        var heights = new float[columns * rows];

        Parallel.For(0, heights.Length, i =>
        {
            var (x, y) = Indexes.ToTwoDimensional(i, columns);
            var noise = FractalBrownianMotion.Generate(SimplexNoise.Noise, x * 0.001f, y * 0.001f, 1.5f, 0.9f, 5);
            noise = Ranges.Map(noise, (-1.0f, 1.0f), (0.0f, MaxHeight));
            heights[i] = (int)noise;
        });

        return heights;
    }

    private static TerrainTile[] GetTiles(float[] heights, int stride)
    {
        var columns = stride;
        var rows = heights.Length / stride;
        var terrain = new TerrainTile[columns * rows];

        // First tile
        terrain[0] = new TerrainTile(heights[0]);

        // First row
        for (var i = 1; i < columns; i++)
        {
            var leftTile = terrain[i - 1];
            var t = TileUtilities.GetNeighboursFromGrid(i, columns, heights);
            terrain[i] = TileUtilities.FitWest(leftTile, t.NW, t.N, t.NE, t.E, t.SW, t.S, t.SE, t.C);
        }

        // First column
        for (var i = stride; i < heights.Length; i += stride)
        {
            var topTile = terrain[i - stride];
            var t = TileUtilities.GetNeighboursFromGrid(i, columns, heights);
            terrain[i] = TileUtilities.FitNorth(topTile, t.NW, t.NE, t.W, t.E, t.SW, t.S, t.SE, t.C);
        }

        // Fill
        for (var i = 0; i < heights.Length; i++)
        {
            var (x, y) = Indexes.ToTwoDimensional(i, stride);
            if (x > 0 && y > 0)
            {
                var tt = TileUtilities.GetNeighboursFromGrid(i, columns, terrain);
                var hh = TileUtilities.GetNeighboursFromGrid(i, columns, heights);
                terrain[i] = TileUtilities.Fit(tt.NW, tt.N, tt.NE, tt.W, hh.E, hh.SW, hh.S, hh.SE, hh.C);
            }
        }

        return terrain;
    }

    private static List<TerrainVertex> GetVertices(IReadOnlyList<TerrainTile> tiles, int columns)
    {
        var vertices = new List<TerrainVertex>(4 * tiles.Count);
        for (var i = 0; i < tiles.Count; i++)
        {
            var tile = tiles[i];
            var (x, y) = Indexes.ToTwoDimensional(i, columns);
            vertices.Add(new TerrainVertex(GetTileCornerPosition(tile, TileCorner.NE, x, y)));
            vertices.Add(new TerrainVertex(GetTileCornerPosition(tile, TileCorner.SE, x, y)));
            vertices.Add(new TerrainVertex(GetTileCornerPosition(tile, TileCorner.SW, x, y)));
            vertices.Add(new TerrainVertex(GetTileCornerPosition(tile, TileCorner.NW, x, y)));
        }

        return vertices;
    }

    private static Vector3 GetTileCornerPosition(TerrainTile tile, TileCorner corner, int tileX, int tileY)
    {
        var offset = TileUtilities.IndexToCorner(tile, corner);
        return new Vector3(offset.X + tileX, offset.Y, offset.Z + tileY);
    }

    private static List<int> GetIndices(IReadOnlyList<TerrainTile> tiles)
    {
        var indices = new List<int>(6 * tiles.Count);

        for (var i = 0; i < tiles.Count; i++)
        {
            var tile = tiles[i];
            var (a, b, c, d, e, f) = TileUtilities.GetBestTriangleIndices(tile);
            var v = i * 4;
            indices.Add(v + a);
            indices.Add(v + b);
            indices.Add(v + c);
            indices.Add(v + d);
            indices.Add(v + e);
            indices.Add(v + f);
        }

        return indices;
    }

    private static int AddGrid(IReadOnlyList<TerrainTile> tiles, List<int> indices)
    {
        var length = tiles.Count * 8;
        indices.EnsureCapacity(indices.Count + length);

        for (var i = 0; i < tiles.Count; i++)
        {
            var v = i * 4;
            indices.Add(v + 0);
            indices.Add(v + 1);

            indices.Add(v + 1);
            indices.Add(v + 2);

            indices.Add(v + 2);
            indices.Add(v + 3);

            indices.Add(v + 3);
            indices.Add(v + 0);
        }

        return length;
    }

    private static List<Triangle> GetTriangles(IReadOnlyList<TerrainTile> tiles, IReadOnlyList<TerrainVertex> vertices, IReadOnlyList<int> indices)
    {
        var triangles = new List<Triangle>(2 * tiles.Count);
        var palette = ColorPalette.GrassLawn;

        for (var i = 0; i < tiles.Count; i++)
        {
            var tile = tiles[i];
            var v = i * 4;
            var ix = i * 6;
            var a = indices[ix + 0] - v;
            var b = indices[ix + 1] - v;
            var c = indices[ix + 2] - v;
            var d = indices[ix + 3] - v;
            var e = indices[ix + 4] - v;
            var f = indices[ix + 5] - v;
            var (n0, n1) = TileUtilities.GetNormals(tile, a, b, c, d, e, f);

            var colorA = GetTriangleColor(palette, vertices, indices[ix + 0], indices[ix + 1], indices[ix + 2]);
            var colorB = GetTriangleColor(palette, vertices, indices[ix + 3], indices[ix + 4], indices[ix + 5]);
            triangles.Add(new Triangle() { Normal = n0, Albedo = colorA });
            triangles.Add(new Triangle() { Normal = n1, Albedo = colorB });
        }

        return triangles;
    }

    private static ColorLinear GetTriangleColor(ColorPalette palette, IReadOnlyList<TerrainVertex> vertices, int a, int b, int c)
    {
        var ya = vertices[a].Position.Y;
        var yb = vertices[b].Position.Y;
        var yc = vertices[c].Position.Y;

        var heigth = Math.Max(ya, Math.Max(yb, yc));
        var paletteIndex = (int)Ranges.Map(heigth, (0.0f, MaxHeight), (0.0f, palette.Colors.Count - 1));
        return Colors.RGBToLinear(palette.Colors[paletteIndex]);
    }

    private static void AddCliffs(IReadOnlyList<TerrainTile> tiles, List<int> indices, List<Triangle> triangles, int columns, int rows)
    {
        for (var it = 0; it < tiles.Count; it++)
        {
            var (x, y) = Indexes.ToTwoDimensional(it, columns);
            if (x > 0)
            {
                AddCliff(tiles, columns, it, TileSide.West, indices, triangles);
            }

            if (x < (columns - 1))
            {
                AddCliff(tiles, columns, it, TileSide.East, indices, triangles);
            }

            if (y > 0)
            {
                AddCliff(tiles, columns, it, TileSide.North, indices, triangles);
            }

            if (y < (rows - 1))
            {
                AddCliff(tiles, columns, it, TileSide.South, indices, triangles);
            }
        }
    }

    private static void AddCliff(IReadOnlyList<TerrainTile> tiles, int stride, int index, TileSide side, List<int> indices, List<Triangle> triangles)
    {
        var (x, y) = Indexes.ToTwoDimensional(index, stride);
        var (nx, ny) = TileUtilities.GetNeighbourIndex(x, y, side);

        // Note: variables are named as if neighbour is current's northern neighbour, but this function works for any neighbour/side

        var cTile = tiles[Indexes.ToOneDimensional(x, y, stride)];
        var nTile = tiles[Indexes.ToOneDimensional(nx, ny, stride)];

        (var cNWCorner, var cNECorner) = TileUtilities.TileSideToTileCorners(side);
        (var nSECorner, var nSWCorner) = TileUtilities.TileSideToTileCorners(TileUtilities.GetOppositeSide(side));

        var cNWHeight = cTile.GetHeight(cNWCorner);
        var cNEHeight = cTile.GetHeight(cNECorner);

        var nSEHeight = nTile.GetHeight(nSECorner);
        var nSWHeight = nTile.GetHeight(nSWCorner);

        // We only care about our sides being higher, the other situations will be taken care of by working on the other tile's sides
        if (cNWHeight > nSWHeight || cNEHeight > nSEHeight) // Cliff
        {
            var cNWIndex = GetVertexIndex(cNWCorner, x, y, stride);
            var cNEIndex = GetVertexIndex(cNECorner, x, y, stride);
            var nSEIndex = GetVertexIndex(nSECorner, nx, ny, stride);
            var nSWIndex = GetVertexIndex(nSWCorner, nx, ny, stride);

            var normal = side switch
            {
                TileSide.North => new Vector3(0.0f, 0.0f, -1.0f),
                TileSide.East => new Vector3(1.0f, 0.0f, 0.0f),
                TileSide.South => new Vector3(0.0f, 0.0f, 1.0f),
                TileSide.West => new Vector3(-1.0f, 0.0f, 0.0f),
                _ => throw new ArgumentOutOfRangeException(nameof(side)),
            };

            var albedo = Colors.RGBToLinear(new ColorRGB(0.5f, 0.25f, 0.20f));

            if (cNWHeight > nSWHeight && cNEHeight > nSEHeight) // A > C && B > D
            {
                indices.EnsureCapacity(indices.Count + 6);
                indices.Add(cNWIndex);
                indices.Add(nSWIndex);
                indices.Add(nSEIndex);
                indices.Add(nSEIndex);
                indices.Add(cNEIndex);
                indices.Add(cNWIndex);

                triangles.Add(new Triangle() { Normal = normal, Albedo = albedo });
                triangles.Add(new Triangle() { Normal = normal, Albedo = albedo });
            }
            else if (cNWHeight > nSWHeight) // A > C
            {
                indices.EnsureCapacity(indices.Count + 3);
                indices.Add(cNWIndex);
                indices.Add(nSWIndex);
                indices.Add(nSEIndex);
                triangles.Add(new Triangle() { Normal = normal, Albedo = albedo });
            }
            else if (cNEHeight > nSEHeight) // B > D
            {
                indices.EnsureCapacity(indices.Count + 3);
                indices.Add(nSWIndex);
                indices.Add(nSEIndex);
                indices.Add(cNEIndex);
                triangles.Add(new Triangle() { Normal = normal, Albedo = albedo });
            }
            else
            {
                throw new Exception("Unexpected case");
            }
        }
    }

    private static int GetVertexIndex(TileCorner corner, int x, int y, int stride)
    {
        return (y * stride * 4) + (x * 4) + (int)corner;
    }

    public void Dispose()
    {
        this.Vertices.Dispose();
        this.Indices.Dispose();

        this.TrianglesView.Dispose();
        this.TrianglesBuffer.Dispose();
    }
}
