using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using LibGame.Geometry;
using LibGame.Graphics;
using LibGame.Mathematics;
using LibGame.Physics;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Contexts.States;
using Mini.Engine.Graphics.Cameras;
using LibGame.Noise;
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
    private readonly IndexBuffer<int> Indices;
    private readonly IndexBuffer<int> GridIndices;
    private readonly VertexBuffer<TerrainVertex> Indicators;
    private readonly VertexBuffer<TerrainVertex> Vertices;
    private readonly StructuredBuffer<Tile> TilesBuffer;
    private readonly ShaderResourceView<Tile> TilesView;
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

        const int width = 1000;
        const int height = 1000;

        this.Indicators = new VertexBuffer<TerrainVertex>(device, nameof(TerrainRenderer));
        var indicators = new TerrainVertex[]
        {
            new(-Vector3.UnitY),
            new(Vector3.UnitY),
        };
        this.Indicators.MapData(device.ImmediateContext, indicators);

        this.Vertices = new VertexBuffer<TerrainVertex>(device, nameof(TerrainRenderer));
        var vertices = GenerateVertices(width, height, 1.0f);
        this.Vertices.MapData(device.ImmediateContext, vertices);

        this.Indices = new IndexBuffer<int>(device, nameof(TerrainRenderer));
        var (indices, triangles) = GenerateTriangles(width, height);
        this.Indices.MapData(device.ImmediateContext, indices);

        this.GridIndices = new IndexBuffer<int>(device, nameof(TerrainRenderer));
        var gridIndices = GenerateGridIndices(width, height);
        this.GridIndices.MapData(device.ImmediateContext, gridIndices);

        var tiles = GenerateTiles(width, height);
        this.TilesBuffer = new StructuredBuffer<Tile>(device, nameof(TerrainRenderer), tiles.Length);
        this.TilesBuffer.MapData(device.ImmediateContext, tiles);
        this.TilesView = this.TilesBuffer.CreateShaderResourceView();

        this.TrianglesBuffer = new StructuredBuffer<Triangle>(device, nameof(TerrainRenderer), triangles.Length);
        this.TrianglesBuffer.MapData(device.ImmediateContext, triangles);
        this.TrianglesView = this.TrianglesBuffer.CreateShaderResourceView();
    }

    private static Tile[] GenerateTiles(int width, int height)
    {
        var palette = ColorPalette.GrassLawn;
        var columns = width - 1;
        var rows = height - 1;
        var length = columns * rows;
        var tiles = new Tile[length];
        for (var i = 0; i < tiles.Length; i++)
        {
            var (x, y) = Indexes.ToTwoDimensional(i, columns);

            var noise = SimplexNoise.Noise(x * 0.01f, y * 0.01f);
            noise = Ranges.Map(noise, (-1.0f, 1.0f), (0.0f, palette.Colors.Count));
            var color = palette.Colors[(int)noise];

            tiles[i] = new Tile()
            {
                Albedo = Colors.RGBToLinear(color),
            };
        }

        return tiles;
    }

    private static (int[], Triangle[]) GenerateTriangles(int width, int height)
    {
        // for every tile we have 2 triangles, so 6 indices
        var tiles = (width - 1) * (height - 1);
        var triangles = new Triangle[tiles * 2];
        var indices = new int[tiles * 6];

        var i = 0;
        var t = 0;
        for (var c = 0; c < tiles; c++)
        {
            var (x, y) = Indexes.ToTwoDimensional(c, width - 1);

            var tl = Indexes.ToOneDimensional(x, y, width);
            var tr = Indexes.ToOneDimensional(x + 1, y, width);
            var bl = Indexes.ToOneDimensional(x, y + 1, width);
            var br = Indexes.ToOneDimensional(x + 1, y + 1, width);

            // Make sure to cut into two triangles so that the
            // edge shared by both triangles is horizontal if possible
            var tlh = GetVertex(x, y);
            var trh = GetVertex(x + 1, y);
            var blh = GetVertex(x, y + 1);
            var brh = GetVertex(x + 1, y + 1);
            if (tlh.Y == brh.Y)
            {
                indices[i++] = tl;
                indices[i++] = tr;
                indices[i++] = br;
                triangles[t++] = new Triangle() { Normal = Triangles.GetNormal(tlh, trh, brh) };

                indices[i++] = br;
                indices[i++] = bl;
                indices[i++] = tl;
                triangles[t++] = new Triangle() { Normal = Triangles.GetNormal(brh, blh, tlh) };
            }
            else
            {
                indices[i++] = tr;
                indices[i++] = br;
                indices[i++] = bl;
                triangles[t++] = new Triangle() { Normal = Triangles.GetNormal(trh, brh, blh) };

                indices[i++] = bl;
                indices[i++] = tl;
                indices[i++] = tr;
                triangles[t++] = new Triangle() { Normal = Triangles.GetNormal(blh, tlh, trh) };
            }
        }

        return (indices, triangles);
    }

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

    private static TerrainVertex[] GenerateVertices(int width, int height, float spacing)
    {
        var vertices = new TerrainVertex[width * height];

        var offset = new Vector3((width - 1) * spacing * -0.5f, 0.0f, (height - 1) * spacing * -0.5f);

        for (var i = 0; i < vertices.Length; i++)
        {
            var (x, y) = Indexes.ToTwoDimensional(i, width);
            var yOffset = GetHeightOffset(x, y);
            var position = offset + GetVertex(x, y, spacing);
            vertices[i] = new TerrainVertex(position);
        }

        return vertices;
    }

    private static Vector3 GetVertex(int x, int y, float spacing = 1.0f)
    {
        var yOffset = GetHeightOffset(x, y);
        return new Vector3(x * spacing, yOffset, y * spacing);
    }

    private static float GetHeightOffset(int x, int y)
    {
        //var noise = FBM(x, y);
        var noise = SimplexNoise.Noise(x * 0.01f, y * 0.01f);
        noise = Ranges.Map(noise, (-1.0f, 1.0f), (0.0f, 10.0f));
        noise = MathF.Floor(noise) * 1.0f;
        var yOffset = noise;

        return yOffset;
    }

    // TODO: replace with new noise
    //private static float FBM(int x, int y)
    //{
    //    const float Frequency = 1.0f;
    //    const float Lacunarity = 0.909f;
    //    const float Amplitude = (1.0f / 256.0f) * 10.0f;
    //    const float Persistance = 0.600f;
    //    const int Octaves = 20;

    //    var sum = 0.0f;

    //    var frequency = Frequency;
    //    var amplitude = Amplitude;
    //    for (var i = 0; i < Octaves; i++)
    //    {
    //        sum += Noise.CalcPixel2D((int)(x * frequency), (int)(y * frequency), 1.0f) * amplitude;
    //        frequency *= Lacunarity;
    //        amplitude *= Persistance;

    //        x += 4643;
    //        y += 3121;
    //    }

    //    return sum;
    //}

    public void Render(DeviceContext context, in PerspectiveCamera camera, in Transform cameraTransform, in Rectangle viewport, in Rectangle scissor)
    {
        this.RenderTiles(context, camera, in cameraTransform, in viewport, in scissor);
        this.RenderSelection(context, in camera, in cameraTransform, in viewport, in scissor);
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

    private void RenderSelection(DeviceContext context, in PerspectiveCamera camera, in Transform cameraTransform, in Rectangle viewport, in Rectangle scissor)
    {
        if (this.Indicators.Length > 0)
        {
            context.Setup(this.Layout, PrimitiveTopology.LineList, this.Shader.Vs, this.CullNoneRasterizerState, in viewport, in scissor, this.Shader.Psline, this.OpaqueBlendState, this.DefaultDepthStencilState);

            context.VS.SetConstantBuffer(Shader.ConstantsSlot, this.User.ConstantsBuffer);

            context.IA.SetVertexBuffer(this.Indicators);
            this.User.MapConstants(context, camera.GetInfiniteReversedZViewProjection(in cameraTransform));

            context.Draw(this.Indicators.Length);
        }
    }

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
        this.Indicators.Dispose();
        this.Vertices.Dispose();
        this.Indices.Dispose();
        this.GridIndices.Dispose();

        this.TilesView.Dispose();
        this.TilesBuffer.Dispose();

        this.TrianglesView.Dispose();
        this.TrianglesBuffer.Dispose();
    }
}
