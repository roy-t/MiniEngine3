using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using LibGame.Graphics;
using LibGame.Mathematics;
using LibGame.Noise;
using LibGame.Physics;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Contexts.States;
using Mini.Engine.Graphics.Cameras;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Shader = Mini.Engine.Content.Shaders.Generated.TitanTerrain;
using Tile = Mini.Engine.Content.Shaders.Generated.TitanTerrain.TILE;
using Triangle = Mini.Engine.Content.Shaders.Generated.TitanTerrain.TRIANGLE;

namespace Mini.Engine.Titan.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct TerrainVertex(Vector3 position)
{
    public readonly Vector3 Position = position;
    public static readonly InputElementDescription[] Elements =
    [
        new("POSITION", 0, Vortice.DXGI.Format.R32G32B32_Float, 0 * sizeof(float), 0, InputClassification.PerVertexData, 0),
    ];

    public override readonly string ToString()
    {
        return this.Position.ToString();
    }
}


[Service]
internal sealed class TerrainRenderer : IDisposable
{
    private const int MaxHeight = 10;

    // private readonly int[] Heights;

    private readonly IndexBuffer<int> Indices;
    private readonly IndexBuffer<int> GridIndices;
    private readonly VertexBuffer<TerrainVertex> Vertices;
    private readonly StructuredBuffer<Tile> TilesBuffer;
    private readonly ShaderResourceView<Tile> TilesView;
    //private readonly VertexBuffer<TerrainVertex> Indicators;
    private readonly StructuredBuffer<Triangle> TrianglesBuffer;
    private readonly ShaderResourceView<Triangle> TrianglesView;
    private readonly InputLayout Layout;
    private readonly Shader Shader;
    private readonly Shader.User User;
    private readonly BlendState OpaqueBlendState;
    private readonly BlendState AlphaBlendState;
    private readonly DepthStencilState DefaultDepthStencilState;
    private readonly DepthStencilState ReadOnlyDepthStencilState;
    private readonly RasterizerState DefaultRasterizerState;
    private readonly RasterizerState CullNoneRasterizerState;

    public TerrainRenderer(Device device, Shader shader)
    {
        this.Layout = shader.CreateInputLayoutForVs(TerrainVertex.Elements);

        this.OpaqueBlendState = device.BlendStates.Opaque;
        this.AlphaBlendState = device.BlendStates.NonPreMultiplied;
        this.DefaultDepthStencilState = device.DepthStencilStates.ReverseZ;
        this.ReadOnlyDepthStencilState = device.DepthStencilStates.ReverseZReadOnly;
        this.DefaultRasterizerState = device.RasterizerStates.Default;
        this.CullNoneRasterizerState = device.RasterizerStates.CullNone;

        this.User = shader.CreateUserFor<TerrainRenderer>();
        this.Shader = shader;


        //this.Indicators = new VertexBuffer<TerrainVertex>(device, nameof(TerrainRenderer));
        //var indicators = new TerrainVertex[]
        //{
        //    new(-Vector3.UnitY),
        //    new(Vector3.UnitY),
        //};
        //this.Indicators.MapData(device.ImmediateContext, indicators);

        const int width = 200;
        const int height = 200;
        var heights = GenerateHeights(width, height);
        //heights[610] = 10.0f;
        //const int width = 3;
        //var heights = new float[]
        //{
        //    0.0f, 0.0f, 0.0f,
        //    0.5f, 0.0f, 0.0f,
        //    0.5f, 0.5f, 0.0f,
        //};



        // TODO: making fitting tiles often fails if there's not fitting shape cascade
        //throw new Exception();
        (var terrainTiles, var tiles) = GetTiles(heights, width);
        //var terrainTiles = new TerrainTile[width * height];
        //for (var i = 0; i < terrainTiles.Length; i++)
        //{
        //    terrainTiles[i] = new TerrainTile(TileType.Flat, heights[i]);
        //}

        //terrainTiles[0] = new TerrainTile(TileType.SlopeN, 1.0f);

        (var vertices, var indices, var gridIndices, var triangles) = FromTiles(terrainTiles, width);

        this.Vertices = new VertexBuffer<TerrainVertex>(device, nameof(TerrainRenderer));
        this.Vertices.MapData(device.ImmediateContext, vertices);

        this.Indices = new IndexBuffer<int>(device, nameof(TerrainRenderer));
        this.Indices.MapData(device.ImmediateContext, indices);

        this.TrianglesBuffer = new StructuredBuffer<Triangle>(device, nameof(TerrainRenderer), triangles.Length);
        this.TrianglesBuffer.MapData(device.ImmediateContext, triangles);
        this.TrianglesView = this.TrianglesBuffer.CreateShaderResourceView();

        this.GridIndices = new IndexBuffer<int>(device, nameof(TerrainRenderer));
        this.GridIndices.MapData(device.ImmediateContext, gridIndices);

        this.TilesBuffer = new StructuredBuffer<Tile>(device, nameof(TerrainRenderer), tiles.Length);
        this.TilesBuffer.MapData(device.ImmediateContext, tiles);
        this.TilesView = this.TilesBuffer.CreateShaderResourceView();
    }

    private static (TerrainTile[], Tile[]) GetTiles(float[] heights, int stride)
    {
        var values = Enum.GetValues<TileType>();
        var options = new List<TileType>();

        var columns = stride;
        var rows = heights.Length / stride;
        var terrain = new TerrainTile[columns * rows];

        var palette = ColorPalette.GrassLawn;
        var tiles = new Tile[columns * rows];
        for (var i = 0; i < tiles.Length; i++)
        {
            var height = heights[i];
            var index = (int)Ranges.Map(height, (0.0f, MaxHeight), (0.0f, palette.Colors.Count - 1));
            var color = palette.Colors[index];
            tiles[i] = new Tile() { Albedo = Colors.RGBToLinear(color) };
        }

        // First tile
        terrain[0] = new TerrainTile(TileType.Flat, heights[0]);

        // First row
        for (var i = 1; i < columns; i++)
        {
            var leftTile = terrain[i - 1];
            var height = heights[i];

            options.Clear();
            options.AddRange(values);
            options = TileUtilities.Fit(TileCorner.NW, leftTile.GetHeight(TileCorner.NE), height, options);
            options = TileUtilities.Fit(TileCorner.SW, leftTile.GetHeight(TileCorner.SE), height, options);

            terrain[i] = new TerrainTile(options.FirstOrDefault(TileType.Flat), height);
        }

        // First column
        for (var i = stride; i < heights.Length; i += stride)
        {
            var topTile = terrain[i - stride];
            var height = heights[i];

            options.Clear();
            options.AddRange(values);
            options = TileUtilities.Fit(TileCorner.NW, topTile.GetHeight(TileCorner.SW), height, options);
            options = TileUtilities.Fit(TileCorner.NE, topTile.GetHeight(TileCorner.SE), height, options);

            terrain[i] = new TerrainTile(options.FirstOrDefault(TileType.Flat), height);
        }

        for (var i = 0; i < heights.Length; i++)
        {
            var (x, y) = Indexes.ToTwoDimensional(i, stride);
            if (x > 0 && y > 0)
            {
                var nw = terrain[i - stride - 1];
                var n = terrain[i - stride];
                var w = terrain[i - 1];
                var height = heights[i];

                options.Clear();
                options.AddRange(values);

                options = TileUtilities.Fit(TileCorner.NW, nw.GetHeight(TileCorner.SE), height, options);

                options = TileUtilities.Fit(TileCorner.NW, n.GetHeight(TileCorner.SW), height, options);
                options = TileUtilities.Fit(TileCorner.NE, n.GetHeight(TileCorner.SE), height, options);

                options = TileUtilities.Fit(TileCorner.NW, w.GetHeight(TileCorner.NE), height, options);
                options = TileUtilities.Fit(TileCorner.SW, w.GetHeight(TileCorner.SE), height, options);

                if (options.Count == 0)
                {
                    tiles[i].Albedo = Colors.RGBToLinear(new ColorRGB(1.0f, 0.0f, 0.0f));
                }

                terrain[i] = new TerrainTile(options.FirstOrDefault(TileType.Flat), height);
            }
        }

        return (terrain, tiles);
    }

    private static (TerrainVertex[], int[], int[], Triangle[]) FromTiles(TerrainTile[] tiles, int stride)
    {
        var vertices = new TerrainVertex[4 * tiles.Length];
        var indices = new int[6 * tiles.Length];
        var gridIndices = new int[8 * tiles.Length];
        var triangles = new Triangle[2 * tiles.Length];

        var v = 0;
        var i = 0;
        var g = 0;
        var t = 0;
        for (var it = 0; it < tiles.Length; it++)
        {
            var tile = tiles[it];
            var (x, y) = Indexes.ToTwoDimensional(it, stride);
            vertices[v + 0] = new TerrainVertex(GetTileCornerPosition(tile, TileCorner.NE, x, y));
            vertices[v + 1] = new TerrainVertex(GetTileCornerPosition(tile, TileCorner.SE, x, y));
            vertices[v + 2] = new TerrainVertex(GetTileCornerPosition(tile, TileCorner.SW, x, y));
            vertices[v + 3] = new TerrainVertex(GetTileCornerPosition(tile, TileCorner.NW, x, y));

            var (a, b, c, d, e, f) = TileUtilities.GetBestTriangleIndices(tile.Type);
            indices[i + 0] = v + a;
            indices[i + 1] = v + b;
            indices[i + 2] = v + c;
            indices[i + 3] = v + d;
            indices[i + 4] = v + e;
            indices[i + 5] = v + f;

            var (n0, n1) = TileUtilities.GetNormals(tile.Type, a, b, c, d, e, f);
            triangles[t + 0] = new Triangle() { Normal = n0 };
            triangles[t + 1] = new Triangle() { Normal = n1 };

            gridIndices[g + 0] = v + 0;
            gridIndices[g + 1] = v + 1;

            gridIndices[g + 2] = v + 1;
            gridIndices[g + 3] = v + 2;

            gridIndices[g + 4] = v + 2;
            gridIndices[g + 5] = v + 3;

            gridIndices[g + 6] = v + 3;
            gridIndices[g + 7] = v + 0;

            v += 4;
            i += 6;
            g += 8;
            t += 2;
        }

        return (vertices, indices, gridIndices, triangles);
    }

    private static Vector3 GetTileCornerPosition(TerrainTile tile, TileCorner corner, int tileX, int tileY)
    {
        var offset = TileUtilities.IndexToCorner(tile.Type, corner);
        return new Vector3(offset.X + tileX, offset.Y + tile.Offset, offset.Z + tileY);
    }

    private static float[] GenerateHeights(int width, int height)
    {
        var heights = new float[width * height];

        Parallel.For(0, heights.Length, i =>
        {
            var (x, y) = Indexes.ToTwoDimensional(i, width);
            var noise = FractalBrownianMotion.Generate(SimplexNoise.Noise, x * 0.001f, y * 0.001f, 1.5f, 0.9f, 5);
            noise = Ranges.Map(noise, (-1.0f, 1.0f), (0.0f, MaxHeight));
            heights[i] = ((int)noise) * 0.5f;
        });

        return heights;
    }

    //private Tile[] GenerateTiles(int width, int height)
    //{
    //    var palette = ColorPalette.GrassLawn;
    //    var columns = width - 1;
    //    var rows = height - 1;
    //    var length = columns * rows;
    //    var tiles = new Tile[length];
    //    for (var i = 0; i < tiles.Length; i++)
    //    {
    //        var (x, y) = Indexes.ToTwoDimensional(i, columns);

    //        var noise = this.GetHeight(x, y, width);
    //        noise = Math.Min(noise, this.GetHeight(x + 1, y, width));
    //        noise = Math.Min(noise, this.GetHeight(x, y + 1, width));
    //        noise = Math.Min(noise, this.GetHeight(x + 1, y + 1, width));
    //        var index = (int)Ranges.Map(noise, (0.0f, MaxHeight), (0.0f, palette.Colors.Count - 1));
    //        var color = palette.Colors[index];

    //        tiles[i] = new Tile()
    //        {
    //            Albedo = Colors.RGBToLinear(color),
    //        };
    //    }

    //    return tiles;
    //}   

    private static int[] GenerateGridIndices(int width, int height)
    {
        var columns = width - 1;
        var rows = height - 1;
        var indices = new int[(columns * 2 * height) + (rows * 2 * width)];

        var i = 0;
        for (var x = 0; x < columns; x++)
        {
            for (var y = 0; y <= rows; y++)
            {
                var a = Indexes.ToOneDimensional(x, y, width);
                var b = Indexes.ToOneDimensional(x + 1, y, width);

                indices[i++] = a;
                indices[i++] = b;
            }
        }

        for (var y = 0; y < rows; y++)
        {
            for (var x = 0; x <= columns; x++)
            {
                var a = Indexes.ToOneDimensional(x, y, width);
                var b = Indexes.ToOneDimensional(x, y + 1, width);

                indices[i++] = a;
                indices[i++] = b;
            }
        }

        return indices;
    }

    public void Render(DeviceContext context, in PerspectiveCamera camera, in Transform cameraTransform, in Rectangle viewport, in Rectangle scissor)
    {
        this.RenderTiles(context, camera, in cameraTransform, in viewport, in scissor);
        //this.RenderSelection(context, in camera, in cameraTransform, in viewport, in scissor);
        this.RenderGrid(context, in camera, in cameraTransform, in viewport, in scissor);
    }

    private void RenderTiles(DeviceContext context, PerspectiveCamera camera, in Transform cameraTransform, in Rectangle viewport, in Rectangle scissor)
    {
        context.Setup(this.Layout, PrimitiveTopology.TriangleList, this.Shader.Vs, this.DefaultRasterizerState, in viewport, in scissor, this.Shader.Ps, this.OpaqueBlendState, this.DefaultDepthStencilState);

        context.VS.SetConstantBuffer(Shader.ConstantsSlot, this.User.ConstantsBuffer);
        context.PS.SetBuffer(Shader.Tiles, this.TilesView);
        context.PS.SetBuffer(Shader.Triangles, this.TrianglesView);

        context.IA.SetVertexBuffer(this.Vertices);
        context.IA.SetIndexBuffer(this.Indices);
        this.User.MapConstants(context, camera.GetInfiniteReversedZViewProjection(in cameraTransform));

        context.DrawIndexed(this.Indices.Length, 0, 0);
    }

    //private void RenderSelection(DeviceContext context, in PerspectiveCamera camera, in Transform cameraTransform, in Rectangle viewport, in Rectangle scissor)
    //{
    //    if (this.Indicators.Length > 0)
    //    {
    //        context.Setup(this.Layout, PrimitiveTopology.LineList, this.Shader.Vs, this.CullNoneRasterizerState, in viewport, in scissor, this.Shader.Psline, this.OpaqueBlendState, this.DefaultDepthStencilState);

    //        context.VS.SetConstantBuffer(Shader.ConstantsSlot, this.User.ConstantsBuffer);

    //        context.IA.SetVertexBuffer(this.Indicators);
    //        this.User.MapConstants(context, camera.GetInfiniteReversedZViewProjection(in cameraTransform));

    //        context.Draw(this.Indicators.Length);
    //    }
    //}

    private void RenderGrid(DeviceContext context, in PerspectiveCamera camera, in Transform cameraTransform, in Rectangle viewport, in Rectangle scissor)
    {
        context.Setup(this.Layout, PrimitiveTopology.LineList, this.Shader.Vs, this.CullNoneRasterizerState, in viewport, in scissor, this.Shader.Psline, this.AlphaBlendState, this.ReadOnlyDepthStencilState);

        context.VS.SetConstantBuffer(Shader.ConstantsSlot, this.User.ConstantsBuffer);

        context.IA.SetVertexBuffer(this.Vertices);
        context.IA.SetIndexBuffer(this.GridIndices);
        this.User.MapConstants(context, camera.GetInfiniteReversedZViewProjection(in cameraTransform));

        context.DrawIndexed(this.GridIndices.Length, 0, 0);
    }

    public void Dispose()
    {
        this.User.Dispose();
        this.Layout.Dispose();
        //this.Indicators.Dispose();
        this.Vertices.Dispose();
        this.Indices.Dispose();
        this.GridIndices.Dispose();
        this.TilesView.Dispose();
        this.TilesBuffer.Dispose();

        this.TrianglesView.Dispose();
        this.TrianglesBuffer.Dispose();
    }
}
