using System;
using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.Content.Shaders;
using Mini.Engine.Content.Shaders.ColorShader;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.ECS.Systems;
using Mini.Engine.Graphics.Transforms;
using Mini.Engine.Graphics.World;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace Mini.Engine.Graphics.Models;

[Service]
public sealed partial class BoundsSystem : ISystem, IDisposable
{

    private readonly Device Device;
    private readonly FrameService FrameService;
    private readonly DebugFrameService DebugFrameService;
    private readonly InputLayout InputLayout;
    private readonly ColorShaderVs VertexShader;
    private readonly ColorShaderPs PixelShader;
    private readonly VertexBuffer<Vector3> VertexBuffer;
    private readonly IndexBuffer<ushort> IndexBuffer;
    private readonly ConstantBuffer<Constants> ConstantBuffer;

    private readonly ImDrawVert[] Buffer;

    public BoundsSystem(Device device, FrameService frameService, DebugFrameService debugFrameService, ColorShaderVs vertexShader, ColorShaderPs pixelShader)
    {
        this.Device = device;
        this.FrameService = frameService;
        this.DebugFrameService = debugFrameService;

        this.VertexShader = vertexShader;
        this.PixelShader = pixelShader;

        this.VertexBuffer = new VertexBuffer<Vector3>(device, $"{nameof(BoundsSystem)}_VB");
        this.IndexBuffer = new IndexBuffer<ushort>(device, $"{nameof(BoundsSystem)}_IB");
        this.IndexBuffer.MapData(this.Device.ImmediateContext,
            0, 1, 1, 2, 2, 3, 3, 0,
            4, 5, 5, 6, 6, 7, 7, 4,
            0, 4, 1, 5, 2, 6, 3, 7
        );

        this.ConstantBuffer = new ConstantBuffer<Constants>(device, $"{nameof(BoundsSystem)}_CB");

        this.InputLayout = this.VertexShader.CreateInputLayout
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

            var cBuffer = new Constants()
            {
                WorldViewProjection = this.FrameService.Camera.ViewProjection,
                Color = Vector4.One
            };
            this.ConstantBuffer.MapData(context, cBuffer);

            context.Setup(this.InputLayout, PrimitiveTopology.LineList, this.VertexShader, this.Device.RasterizerStates.CullNone, 0, 0, this.Device.Width, this.Device.Height, this.PixelShader, this.Device.BlendStates.Opaque, this.Device.DepthStencilStates.None);
            context.OM.SetRenderTarget(this.DebugFrameService.DebugOverlay);

            context.IA.SetVertexBuffer(this.VertexBuffer);
            context.IA.SetIndexBuffer(this.IndexBuffer);
            context.VS.SetConstantBuffer(Constants.Slot, this.ConstantBuffer);
            context.PS.SetConstantBuffer(Constants.Slot, this.ConstantBuffer);
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
            var bounds = component.Mesh.Bounds.Transform(world);

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
        this.ConstantBuffer.Dispose();
        this.InputLayout.Dispose();
    }
}
