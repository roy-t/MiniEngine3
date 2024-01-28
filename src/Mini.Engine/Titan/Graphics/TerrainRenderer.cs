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
    private const int bias = -10000;

    private readonly InputLayout Layout;
    private readonly Shader Shader;
    private readonly Shader.User User;
    private readonly BlendState OpaqueBlendState;
    private readonly BlendState AlphaBlendState;
    private readonly DepthStencilState DefaultDepthStencilState;
    private readonly DepthStencilState ReadOnlyDepthStencilState;
    private readonly RasterizerState GridRasterizerState;
    private readonly RasterizerState TerrainRasterizerState;

    public TerrainRenderer(Device device, Shader shader)
    {
        this.Layout = shader.CreateInputLayoutForVs(TerrainVertex.Elements);

        this.OpaqueBlendState = device.BlendStates.Opaque;
        this.AlphaBlendState = device.BlendStates.NonPreMultiplied;
        this.DefaultDepthStencilState = device.DepthStencilStates.ReverseZ;
        this.ReadOnlyDepthStencilState = device.DepthStencilStates.ReverseZReadOnly;
        this.GridRasterizerState = device.RasterizerStates.CullNone;
        // Make sure the terrain is always the lowest object so it doesn't interfer with the grid
        this.TerrainRasterizerState = RasterizerStates.CreateBiased(device, device.RasterizerStates.Default, bias);
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

        context.Setup(this.Layout, PrimitiveTopology.TriangleList, this.Shader.Vs, this.TerrainRasterizerState, in viewport, in scissor, this.Shader.Ps, this.OpaqueBlendState, this.DefaultDepthStencilState);
        context.DrawIndexed(terrain.TileIndexCount, terrain.TileIndexOffset, 0);

        context.Setup(this.Layout, PrimitiveTopology.LineList, this.Shader.Vs, this.GridRasterizerState, in viewport, in scissor, this.Shader.Psline, this.AlphaBlendState, this.ReadOnlyDepthStencilState);
        context.DrawIndexed(terrain.GridIndexCount, terrain.GridIndexOffset, 0);
    }

    public void Dispose()
    {
        this.User.Dispose();
        this.Layout.Dispose();
        this.TerrainRasterizerState.Dispose();
    }
}