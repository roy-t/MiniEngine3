using System.Drawing;
using LibGame.Physics;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Contexts.States;
using Mini.Engine.Graphics.Cameras;
using Vortice.Direct3D;
using Shader = Mini.Engine.Content.Shaders.Generated.TitanTerrain;

namespace Mini.Engine.Titan.Graphics;

[Service]
public sealed class TerrainRenderer : IDisposable
{
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
        this.User = shader.CreateUserFor<Terrain>();
        this.Shader = shader;
    }

    public void Setup(DeviceContext context, in PerspectiveCamera camera, in Transform cameraTransform)
    {
        context.VS.SetConstantBuffer(Shader.ConstantsSlot, this.User.ConstantsBuffer);
        context.VS.SetConstantBuffer(Shader.ConstantsSlot, this.User.ConstantsBuffer);
        this.User.MapConstants(context, camera.GetInfiniteReversedZViewProjection(in cameraTransform));
    }

    public void Render(DeviceContext context, in Rectangle viewport, in Rectangle scissor, ITerrain terrain)
    {
        context.PS.SetBuffer(Shader.Triangles, terrain.TrianglesView);
        context.IA.SetVertexBuffer(terrain.Vertices);
        context.IA.SetIndexBuffer(terrain.Indices);

        context.Setup(this.Layout, PrimitiveTopology.TriangleList, this.Shader.Vs, this.RasterizerState, in viewport, in scissor, this.Shader.Ps, this.BlendState, this.DepthStencilState);
        context.DrawIndexed(terrain.TileIndexCount, terrain.TileIndexOffset, 0);
    }

    public void Dispose()
    {
        this.User.Dispose();
        this.Layout.Dispose();
    }
}