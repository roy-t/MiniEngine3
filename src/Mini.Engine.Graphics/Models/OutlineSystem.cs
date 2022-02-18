using System;
using ImGuiNET;
using Mini.Engine.Content;
using Mini.Engine.Content.Shaders;
using Mini.Engine.Content.Shaders.DebugLines;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Mini.Engine.ECS.Systems;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.Graphics.Transforms;
using System.Diagnostics;
using Vortice.Direct3D;
using System.Numerics;
using Mini.Engine.Configuration;

namespace Mini.Engine.Graphics.Models;

[Service]
public sealed partial class OutlineSystem : ISystem, IDisposable
{

    private readonly Device Device;
    private readonly FrameService FrameService;
    private readonly InputLayout InputLayout;
    private readonly DebugLinesVs VertexShader;
    private readonly DebugLinesPs PixelShader;
    private readonly VertexBuffer<Vector3> VertexBuffer;
    private readonly IndexBuffer<ushort> IndexBuffer;
    private readonly ConstantBuffer<Constants> ConstantBuffer;

    private readonly ImDrawVert[] Buffer;

    public OutlineSystem(Device device, FrameService frameService, ContentManager content)
    {
        this.Device = device;

        this.VertexShader = content.LoadDebugLinesVs();
        this.PixelShader = content.LoadDebugLinesPs();

        this.VertexBuffer = new VertexBuffer<Vector3>(device, $"{nameof(OutlineSystem)}_VB");
        this.IndexBuffer = new IndexBuffer<ushort>(device, $"{nameof(OutlineSystem)}_IB");
        this.IndexBuffer.MapData(this.Device.ImmediateContext, 
            0, 1, 1, 2, 2, 3, 3, 0,
            4, 5, 5, 6, 6, 7, 7, 4,
            0, 4, 1, 5, 2, 6, 3, 7
        );

        this.ConstantBuffer = new ConstantBuffer<Constants>(device, $"{nameof(OutlineSystem)}_CB");

        this.InputLayout = this.VertexShader.CreateInputLayout
        (
            device,
            new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0)
        );
        this.FrameService = frameService;

        this.Buffer = new ImDrawVert[12];
    }

    public void OnSet()
    {
        var context = this.Device.ImmediateContext;

        context.Setup(this.InputLayout, this.VertexShader, this.PixelShader, this.Device.BlendStates.Opaque, this.Device.DepthStencilStates.None, this.Device.Width, this.Device.Height);

        context.RS.SetRasterizerState(this.Device.RasterizerStates.CullNone); // useless?
        context.IA.SetPrimitiveTopology(PrimitiveTopology.LineList);
        context.VS.SetConstantBuffer(Constants.Slot, this.ConstantBuffer);
        context.OM.SetRenderTarget(this.FrameService.LBuffer.Light);
    }
    
    [Process(Query = ProcessQuery.All)]
    public void DrawOutline(ModelComponent component, TransformComponent transform)
    {
        var context = this.Device.ImmediateContext;

        var world = transform.AsMatrix();
        var bounds = component.Model.Bounds.Transform(world);

        var cBuffer = new Constants()
        {
            WorldViewProjection = this.FrameService.Camera.ViewProjection,
            Color = Vector4.One
        };
        this.ConstantBuffer.MapData(context, cBuffer);

        var corners = bounds.GetCorners();        
        this.VertexBuffer.MapData(context, corners);

        context.IA.SetVertexBuffer(this.VertexBuffer);
        context.IA.SetIndexBuffer(this.IndexBuffer);

        context.DrawIndexed(24, 0, 0);

        // TODO: draw meshes!
        

        //for (var i = 0; i < component.Model.Primitives.Length; i++)
        //{
        //    var primitive = component.Model.Primitives[i];
        //    var material = component.Model.Materials[primitive.MaterialIndex];

        //    this.Context.PS.SetShaderResource(Geometry.Albedo, material.Albedo);
        //    this.Context.PS.SetShaderResource(Geometry.Normal, material.Normal);
        //    this.Context.PS.SetShaderResource(Geometry.Metalicness, material.Metalicness);
        //    this.Context.PS.SetShaderResource(Geometry.Roughness, material.Roughness);
        //    this.Context.PS.SetShaderResource(Geometry.AmbientOcclusion, material.AmbientOcclusion);
        //    this.Context.DrawIndexed(primitive.IndexCount, primitive.IndexOffset, 0);
        //}
    }

    public void OnUnSet() { }

    public void Dispose()
    {
        this.ConstantBuffer.Dispose();
        this.InputLayout.Dispose();
        this.PixelShader.Dispose();
        this.VertexShader.Dispose();
    }
}
