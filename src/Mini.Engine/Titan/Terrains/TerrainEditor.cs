using System.Drawing;
using System.Numerics;
using LibGame.Mathematics;
using LibGame.Physics;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Contexts.States;
using Mini.Engine.Graphics.Cameras;
using Mini.Engine.Windows;
using Vortice.Direct3D;
using Vortice.Mathematics;
using Shader = Mini.Engine.Content.Shaders.Generated.TitanGizmo;

namespace Mini.Engine.Titan.Terrains;

[Service]
public sealed class TerrainEditor : IDisposable
{
    private readonly InputLayout Layout;
    private readonly Shader Shader;
    private readonly InputService InputService;
    private readonly Shader.User User;
    private readonly BlendState BlendState;
    private readonly DepthStencilState DepthStencilState;
    private readonly RasterizerState RasterizerState;

    private readonly VertexBuffer<GizmoVertex> Vertices;
    private readonly IndexBuffer<int> Indices;

    private Vector3 position;

    public TerrainEditor(Device device, Shader shader, InputService inputService)
    {
        this.Layout = shader.CreateInputLayoutForVs(GizmoVertex.Elements);

        this.BlendState = device.BlendStates.Opaque;
        this.DepthStencilState = device.DepthStencilStates.None;
        this.RasterizerState = device.RasterizerStates.Default;
        this.User = shader.CreateUserFor<TerrainEditor>();
        this.Shader = shader;
        this.InputService = inputService;

        var vertices = new GizmoVertex[]
        {
            // Top, starting NE
            new(new Vector3(0.5f, 0.5f, -0.5f)),
            new(new Vector3(0.5f, 0.5f, 0.5f)),
            new(new Vector3(-0.5f, 0.5f, 0.5f)),
            new(new Vector3(-0.5f, 0.5f, -0.5f)),

            // Bottom, starting NE
            new(new Vector3(0.5f, -0.5f, -0.5f)),
            new(new Vector3(0.5f, -0.5f, 0.5f)),
            new(new Vector3(-0.5f, -0.5f, 0.5f)),
            new(new Vector3(-0.5f, -0.5f, -0.5f)),
        };

        var indices = new int[]
        {
            // Top
            0, 1, 2,
            2, 3, 0,

            // N
            3, 7, 4,
            4, 0, 3,

            // E
            0, 4, 5,
            5, 1, 0,

            // S
            1, 5, 6,
            6, 2, 1,

            // W
            2, 6, 7,
            7, 3, 2,

            // Note: you will never see the bottom, so let's not draw it
        };

        this.Vertices = new VertexBuffer<GizmoVertex>(device, nameof(TerrainEditor));
        this.Vertices.MapData(device.ImmediateContext, vertices);

        this.Indices = new IndexBuffer<int>(device, nameof(TerrainEditor));
        this.Indices.MapData(device.ImmediateContext, indices);
    }

    public void CaptureMouse(Terrain terrain, in Rectangle viewport, in PerspectiveCamera camera, in Transform cameraTransform)
    {
        var cursor = this.InputService.GetCursorPosition();
        if (viewport.Contains((int)cursor.X, (int)cursor.Y))
        {
            var wvp = camera.GetViewProjection(in cameraTransform);
            var (position, direction) = Picking.CalculateCursorRay(cursor, in viewport, in wvp);
            var ray = new Ray(position, direction);
            if (terrain.Bounds.CheckTileHit(ray, out var tileIndex, out var pit))
            {
                this.position = pit;
            }
        }
    }

    public void Setup(DeviceContext context, in PerspectiveCamera camera, in Transform cameraTransform)
    {
        context.VS.SetConstantBuffer(Shader.ConstantsSlot, this.User.ConstantsBuffer);
        context.VS.SetConstantBuffer(Shader.ConstantsSlot, this.User.ConstantsBuffer);

        var world = Matrix4x4.CreateTranslation(this.position);
        var wvp = Matrix4x4.Multiply(world, camera.GetInfiniteReversedZViewProjection(in cameraTransform));
        this.User.MapConstants(context, wvp);
    }

    public void Render(DeviceContext context, in Rectangle viewport, in Rectangle scissor)
    {
        context.IA.SetVertexBuffer(this.Vertices);
        context.IA.SetIndexBuffer(this.Indices);

        context.Setup(this.Layout, PrimitiveTopology.TriangleList, this.Shader.Vs, this.RasterizerState, in viewport, in scissor, this.Shader.Ps, this.BlendState, this.DepthStencilState);
        context.DrawIndexed(this.Indices.Length, 0, 0);
    }

    public void Dispose()
    {
        this.User.Dispose();
        this.Layout.Dispose();
        this.Vertices.Dispose();
        this.Indices.Dispose();
    }
}
