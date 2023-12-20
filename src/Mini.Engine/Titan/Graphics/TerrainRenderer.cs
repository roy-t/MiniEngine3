using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using LibGame.Graphics;
using LibGame.Mathematics;
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

namespace Mini.Engine.Titan.Graphics;


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TerrainVertex
{
    public Vector3 Position;

    public TerrainVertex(Vector3 position)
    {
        this.Position = position;
    }

    public static readonly InputElementDescription[] Elements =
    {
        new("POSITION", 0, Vortice.DXGI.Format.R32G32B32_Float, 0 * sizeof(float), 0, InputClassification.PerVertexData, 0),
    };

    public override readonly string ToString()
    {
        return this.Position.ToString();
    }
}

[Service]
internal sealed class TerrainRenderer : IDisposable
{
    private readonly IndexBuffer<int> Indices;
    private readonly VertexBuffer<TerrainVertex> Vertices;
    private readonly StructuredBuffer<Tile> Tiles;
    private readonly ShaderResourceView<Tile> TilesView;
    private readonly InputLayout Layout;
    private readonly Shader Shader;
    private readonly Shader.User User;
    private readonly BlendState BlendState;
    private readonly DepthStencilState DepthStencilState;
    private readonly RasterizerState RasterizerState;

    public TerrainRenderer(Device device, Shader shader)
    {
        this.Layout = shader.CreateInputLayoutForVs(TerrainVertex.Elements);

        this.BlendState = device.BlendStates.Opaque;
        this.DepthStencilState = device.DepthStencilStates.ReverseZ;
        this.RasterizerState = device.RasterizerStates.Default;

        this.User = shader.CreateUserFor<TerrainRenderer>();
        this.Shader = shader;

        const int width = 1000;
        const int height = 1000;

        this.Vertices = new VertexBuffer<TerrainVertex>(device, nameof(TerrainRenderer));
        var vertices = GenerateVertices(width, height, 1.0f);
        this.Vertices.MapData(device.ImmediateContext, vertices);

        this.Indices = new IndexBuffer<int>(device, nameof(TerrainRenderer));
        var indices = GenerateIndices(width, height);
        this.Indices.MapData(device.ImmediateContext, indices);


        var tiles = GenerateTiles(width, height);
        this.Tiles = new StructuredBuffer<Tile>(device, nameof(TerrainRenderer), tiles.Length);
        this.Tiles.MapData(device.ImmediateContext, tiles);
        this.TilesView = this.Tiles.CreateShaderResourceView();
    }

    private static Tile[] GenerateTiles(int width, int height)
    {
        var length = (width - 1) * (height - 1);
        var tiles = new Tile[length];
        for (var i = 0; i < tiles.Length; i++)
        {
            var color = Colors.RGBToLinear(ColorPalette.GrassLawn.Pick());
            tiles[i] = new Tile()
            {
                Albedo = color
            };

        }

        return tiles;
    }

    private static int[] GenerateIndices(int width, int height)
    {
        // for every tile we have 2 triangles, so 6 indices
        var tiles = (width - 1) * (height - 1);
        var indices = new int[tiles * 6];

        var i = 0;
        for (var c = 0; c < tiles; c++)
        {
            var (x, y) = Indexes.ToTwoDimensional(c, width - 1);

            var tl = Indexes.ToOneDimensional(x, y, width);
            var tr = Indexes.ToOneDimensional(x + 1, y, width);
            var bl = Indexes.ToOneDimensional(x, y + 1, width);
            var br = Indexes.ToOneDimensional(x + 1, y + 1, width);

            indices[i++] = tl;
            indices[i++] = tr;
            indices[i++] = br;

            indices[i++] = br;
            indices[i++] = bl;
            indices[i++] = tl;
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
            var position = offset + new Vector3(x * spacing, 0.0f, y * spacing);
            vertices[i] = new TerrainVertex(position);
        }

        return vertices;
    }

    public void Render(DeviceContext context, in PerspectiveCamera camera, in Transform cameraTransform, in Rectangle viewport, in Rectangle scissor)
    {
        context.Setup(this.Layout, PrimitiveTopology.TriangleList, this.Shader.Vs, this.RasterizerState, in viewport, in scissor, this.Shader.Ps, this.BlendState, this.DepthStencilState);

        context.VS.SetConstantBuffer(Shader.ConstantsSlot, this.User.ConstantsBuffer);
        context.PS.SetConstantBuffer(Shader.ConstantsSlot, this.User.ConstantsBuffer);
        context.PS.SetBuffer(Shader.Tiles, this.TilesView);

        context.IA.SetVertexBuffer(this.Vertices);
        context.IA.SetIndexBuffer(this.Indices);
        this.User.MapConstants(context, camera.GetInfiniteReversedZViewProjection(in cameraTransform));

        context.DrawIndexed(this.Indices.Length, 0, 0);
    }

    public void Dispose()
    {
        this.User.Dispose();
        this.Layout.Dispose();
        this.Vertices.Dispose();
        this.Indices.Dispose();

        this.TilesView.Dispose();
        this.Tiles.Dispose();        
    }
}
