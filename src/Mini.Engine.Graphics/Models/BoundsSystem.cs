using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.Content.Shaders.Generated;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.ECS.Systems;
using Mini.Engine.Graphics.Transforms;
using Mini.Engine.Graphics.World;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.Graphics.Models;

[Service]
public sealed partial class BoundsSystem : ISystem, IDisposable
{

    private readonly Device Device;
    private readonly FrameService FrameService;
    private readonly DebugFrameService DebugFrameService;
    private readonly InputLayout InputLayout;
    private readonly ColorShader Shader;
    private readonly ColorShader.User User;
    private readonly VertexBuffer<Vector3> VertexBuffer;
    private readonly IndexBuffer<ushort> IndexBuffer;

    private readonly ImDrawVert[] Buffer;

    public BoundsSystem(Device device, FrameService frameService, DebugFrameService debugFrameService, ColorShader shader)
    {
        this.Device = device;
        this.FrameService = frameService;
        this.DebugFrameService = debugFrameService;

        this.Shader = shader;
        this.User = shader.CreateUserFor<BoundsSystem>();

        this.VertexBuffer = new VertexBuffer<Vector3>(device, $"{nameof(BoundsSystem)}_VB");
        this.IndexBuffer = new IndexBuffer<ushort>(device, $"{nameof(BoundsSystem)}_IB");
        this.IndexBuffer.MapData(this.Device.ImmediateContext,
            0, 1, 1, 2, 2, 3, 3, 0,
            4, 5, 5, 6, 6, 7, 7, 4,
            0, 4, 1, 5, 2, 6, 3, 7
        );

        this.InputLayout = this.Shader.Vs.CreateInputLayout
        (
            device,
            new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0)
        );

        this.Buffer = new ImDrawVert[12];
    }

    public void OnSet()
    {
        if (this.DebugFrameService.ShowBounds)
        {
            var context = this.Device.ImmediateContext;
            
            this.User.MapConstants(context, this.FrameService.Camera.ViewProjection, Vector4.One);

            context.Setup(this.InputLayout, PrimitiveTopology.LineList, this.Shader.Vs, this.Device.RasterizerStates.CullNone, 0, 0, this.Device.Width, this.Device.Height, this.Shader.Ps, this.Device.BlendStates.Opaque, this.Device.DepthStencilStates.None);
            context.OM.SetRenderTarget(this.DebugFrameService.DebugOverlay);

            context.IA.SetVertexBuffer(this.VertexBuffer);
            context.IA.SetIndexBuffer(this.IndexBuffer);
            context.VS.SetConstantBuffer(ColorShader.ConstantsSlot, this.User.ConstantsBuffer);
            context.PS.SetConstantBuffer(ColorShader.ConstantsSlot, this.User.ConstantsBuffer);
        }
    }

    [Process(Query = ProcessQuery.All)]
    public void DrawOutline(ModelComponent component, TransformComponent transform)
    {
        if (this.DebugFrameService.ShowBounds)
        {
            var camera = this.FrameService.Camera;

            var world = transform.AsMatrix();
            var bounds = component.Model.Bounds.Transform(world);

            if (camera.Frustum.ContainsOrIntersects(bounds))
            {
                var context = this.Device.ImmediateContext;
                var corners = bounds.GetCorners();
                this.VertexBuffer.MapData(context, corners);

                context.DrawIndexed(24, 0, 0);

                for (var i = 0; i < component.Model.Primitives.Length; i++)
                {
                    var primitive = component.Model.Primitives[i];

                    bounds = primitive.Bounds.Transform(world);
                    if (camera.Frustum.ContainsOrIntersects(bounds))
                    {
                        corners = bounds.GetCorners();
                        this.VertexBuffer.MapData(context, corners);
                        context.DrawIndexed(24, 0, 0);
                    }
                }
            }
        }
    }

    [Process(Query = ProcessQuery.All)]
    public void DrawOutline(TerrainComponent component, TransformComponent transform)
    {
        if (this.DebugFrameService.ShowBounds)
        {
            var camera = this.FrameService.Camera;

            var world = transform.AsMatrix();
            var bounds = component.Terrain.Mesh.Bounds.Transform(world);

            if (camera.Frustum.ContainsOrIntersects(bounds))
            {
                var context = this.Device.ImmediateContext;
                var corners = bounds.GetCorners();
                this.VertexBuffer.MapData(context, corners);

                context.DrawIndexed(24, 0, 0);
            }
        }
    }   

    public void OnUnSet() { }

    public void Dispose()
    {
        this.User.Dispose();
        this.InputLayout.Dispose();
    }
}
