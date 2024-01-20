﻿using System.Drawing;
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

    private readonly IndexBuffer<int> Indices;
    private readonly IndexBuffer<int> GridIndices;
    private readonly VertexBuffer<TerrainVertex> Vertices;
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
        var heightMap = GenerateHeightMap(width, height);
        heightMap[width + 4] += 4;
        var tiles = GetTilesFromHeightMap(heightMap, width);

        (var vertices, var indices, var gridIndices, var triangles) = GetRenderDataFromTiles(tiles, width);

        this.Vertices = new VertexBuffer<TerrainVertex>(device, nameof(TerrainRenderer));
        this.Vertices.MapData(device.ImmediateContext, vertices);

        this.Indices = new IndexBuffer<int>(device, nameof(TerrainRenderer));
        this.Indices.MapData(device.ImmediateContext, indices);

        this.TrianglesBuffer = new StructuredBuffer<Triangle>(device, nameof(TerrainRenderer), triangles.Length);
        this.TrianglesBuffer.MapData(device.ImmediateContext, triangles);
        this.TrianglesView = this.TrianglesBuffer.CreateShaderResourceView();

        this.GridIndices = new IndexBuffer<int>(device, nameof(TerrainRenderer));
        this.GridIndices.MapData(device.ImmediateContext, gridIndices);
    }

    private static TerrainTile[] GetTilesFromHeightMap(float[] heights, int stride)
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

    private static ReadOnlySpan<int> GetCliffIndicesFromTiles(TerrainTile[] tiles, int stride)
    {
        var indices = new List<int>();

        for (var it = 0; it < tiles.Length; it++)
        {
            var tile = tiles[it];
            var (x, y) = Indexes.ToTwoDimensional(it, stride);
            var neighbours = TileUtilities.GetNeighboursFromGrid(it, stride, tiles);

            var nw = tile.GetHeight(TileCorner.NW);
            var sw = tile.GetHeight(TileCorner.SW);

            var wnw = neighbours.W.GetHeight(TileCorner.NE);
            var wsw = neighbours.W.GetHeight(TileCorner.SE);

            // TODO: floating point comparisons might fail, but solved when we turn to bytes
            // TODO: guard for out of bounds
            if (wnw != nw && sw != wsw)
            {
                var a = GetVertexIndex(TileCorner.SW, x, y, stride);
                var b = GetVertexIndex(TileCorner.SE, x - 1, y, stride);
                var c = GetVertexIndex(TileCorner.NE, x - 1, y, stride);
                var d = GetVertexIndex(TileCorner.NW, x, y, stride);
                indices.AddRange([a, b, c, c, d, a]);
            }
        }

        return CollectionsMarshal.AsSpan(indices);
    }

    private static void CheckCliff(TerrainTile[] tiles, int stride, int index, TileSide side)
    {
        var (x, y) = Indexes.ToTwoDimensional(index, stride);
        var (nx, ny) = side switch
        {
            TileSide.North => (x + 0, y - 1),
            TileSide.East => (x + 1, y + 0),
            TileSide.South => (x + 0, y + 1),
            TileSide.West => (x - 1, y + 0),
            _ => throw new ArgumentOutOfRangeException(nameof(side));
        };

        if (nx >= 0 && nx < stride && ny >= 0 && (ny * stride) < tiles.Length)
        {
            CheckCliffSide(tiles, stride, x, y, nx, ny, side);
        }
    }

    private static ReadOnlySpan<int> CheckCliffSide(TerrainTile[] tiles, int stride, int cx, int cy, int nx, int ny, TileSide side, List<int> indices)
    {
        var c = tiles[Indexes.ToOneDimensional(cx, cy, stride)];
        var n = tiles[Indexes.ToOneDimensional(nx, ny, stride)];

        (var ca, var cb) = TileUtilities.TileSideToTileCorners(side);
        (var na, var nb) = TileUtilities.TileSideToTileCorners(TileUtilities.GetOppositeSide(side));

        var ica = GetVertexIndex(ca, cx, cy, stride);
        var icb = GetVertexIndex(cb, cx, cy, stride);

        var ina = GetVertexIndex(na, nx, ny, stride);
        var inb = GetVertexIndex(nb, nx, ny, stride);

        var hca = c.GetHeight(ca);
        var hcb = c.GetHeight(cb);

        var hna = c.GetHeight(na);
        var hnb = c.GetHeight(nb);

        // depending on which side is higher we need to draw the indices in a different order!

        if (hca > hnb)
        {
            if (hcb > hna)
            {
                indices.AddRange([ica, inb, ina, ina, icb, ica]);
            }
        }

        // TODO: other cases
        TERJALKTJHELAKJTLKEAJTLKJEALKTJAELKJTLKE HIER WAS IK!

        return [];
    }

    private static int GetVertexIndex(TileCorner corner, int x, int y, int stride)
    {
        return (y * stride * 4) + (x * 4) + (int)corner;
    }

    private static (TerrainVertex[] vertices, int[] indices, int[] gridIndices, Triangle[] triangles) GetRenderDataFromTiles(TerrainTile[] tiles, int stride)
    {
        var palette = ColorPalette.GrassLawn;
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
            // Don't change this order without changing GetVertexIndex
            vertices[v + 0] = new TerrainVertex(GetTileCornerPosition(tile, TileCorner.NE, x, y));
            vertices[v + 1] = new TerrainVertex(GetTileCornerPosition(tile, TileCorner.SE, x, y));
            vertices[v + 2] = new TerrainVertex(GetTileCornerPosition(tile, TileCorner.SW, x, y));
            vertices[v + 3] = new TerrainVertex(GetTileCornerPosition(tile, TileCorner.NW, x, y));

            var (a, b, c, d, e, f) = TileUtilities.GetBestTriangleIndices(tile);
            indices[i + 0] = v + a;
            indices[i + 1] = v + b;
            indices[i + 2] = v + c;
            indices[i + 3] = v + d;
            indices[i + 4] = v + e;
            indices[i + 5] = v + f;

            var (n0, n1) = TileUtilities.GetNormals(tile, a, b, c, d, e, f);

            var colorA = GetTriangleColor(palette, vertices, indices[i + 0], indices[i + 1], indices[i + 2]);
            triangles[t + 0] = new Triangle() { Normal = n0, Albedo = colorA };

            var colorB = GetTriangleColor(palette, vertices, indices[i + 3], indices[i + 4], indices[i + 5]);
            triangles[t + 1] = new Triangle() { Normal = n1, Albedo = colorB };

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

    private static ColorLinear GetTriangleColor(ColorPalette palette, TerrainVertex[] vertices, int a, int b, int c)
    {
        var ya = vertices[a].Position.Y;
        var yb = vertices[b].Position.Y;
        var yc = vertices[c].Position.Y;

        var heigth = Math.Max(ya, Math.Max(yb, yc));
        var paletteIndex = (int)Ranges.Map(heigth, (0.0f, MaxHeight), (0.0f, palette.Colors.Count - 1));
        return Colors.RGBToLinear(palette.Colors[paletteIndex]);
    }

    private static Vector3 GetTileCornerPosition(TerrainTile tile, TileCorner corner, int tileX, int tileY)
    {
        var offset = TileUtilities.IndexToCorner(tile, corner);
        return new Vector3(offset.X + tileX, offset.Y, offset.Z + tileY);
    }

    private static float[] GenerateHeightMap(int width, int height)
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

        this.TrianglesView.Dispose();
        this.TrianglesBuffer.Dispose();
    }
}
