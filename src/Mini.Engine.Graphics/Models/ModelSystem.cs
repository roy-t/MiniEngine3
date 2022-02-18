using System;
using System.Diagnostics;
using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.Content.Shaders;
using Mini.Engine.Content.Shaders.Geometry;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.ECS.Systems;
using Mini.Engine.Graphics.Transforms;
using Vortice.Direct3D;
using Vortice.Mathematics;

namespace Mini.Engine.Graphics.Models;

[Service]
public sealed partial class ModelSystem : ISystem, IDisposable
{
    private readonly Device Device;
    private readonly DeferredDeviceContext Context;
    private readonly FrameService FrameService;
    private readonly GeometryVs VertexShader;
    private readonly GeometryPs PixelShader;
    private readonly InputLayout InputLayout;
    private readonly ConstantBuffer<Constants> ConstantBuffer;


    public ModelSystem(Device device, FrameService frameService, ContentManager content)
    {
        this.Device = device;
        this.Context = device.CreateDeferredContextFor<ModelSystem>();
        this.FrameService = frameService;
        this.VertexShader = content.LoadGeometryVs();
        this.PixelShader = content.LoadGeometryPs();
        this.InputLayout = this.VertexShader.CreateInputLayout(device, ModelVertex.Elements);
        this.ConstantBuffer = new ConstantBuffer<Constants>(device, $"{nameof(ModelSystem)}_CB");
    }

    public void OnSet()
    {
        var width = this.FrameService.GBuffer.Width;
        var height = this.FrameService.GBuffer.Height;

        this.Context.IA.SetInputLayout(this.InputLayout);
        this.Context.IA.SetPrimitiveTopology(PrimitiveTopology.TriangleList);

        this.Context.VS.SetShader(this.VertexShader);

        this.Context.RS.SetViewPort(0, 0, width, height);
        this.Context.RS.SetScissorRect(0, 0, width, height);
        this.Context.RS.SetRasterizerState(this.Device.RasterizerStates.CullCounterClockwise);

        this.Context.PS.SetShader(this.PixelShader);
        this.Context.PS.SetSampler(Geometry.TextureSampler, this.Device.SamplerStates.AnisotropicWrap);

        this.Context.OM.SetRenderTargets(this.FrameService.GBuffer.DepthStencilBuffer, this.FrameService.GBuffer.Albedo, this.FrameService.GBuffer.Material, this.FrameService.GBuffer.Normal);

        this.Context.OM.SetBlendState(this.Device.BlendStates.Opaque);
        this.Context.OM.SetDepthStencilState(this.Device.DepthStencilStates.Default);
    }

    [Process(Query = ProcessQuery.All)]
    public void DrawModel(ModelComponent component, TransformComponent transform)
    {
        var world = transform.AsMatrix();
        var bounds = Transform(component.Model.Bounds, world);
        var frustum = new Frustum(this.FrameService.Camera.ViewProjection);

        if (frustum.ContainsOrIntersects(bounds))
        {
            Debug.WriteLine("SEEN");
            var cBuffer = new Constants()
            {
                WorldViewProjection = world * this.FrameService.Camera.ViewProjection,
                World = world,
                CameraPosition = this.FrameService.Camera.Transform.Position
            };
            this.ConstantBuffer.MapData(this.Context, cBuffer);
            this.Context.VS.SetConstantBuffer(Constants.Slot, this.ConstantBuffer);
            this.Context.PS.SetConstantBuffer(Constants.Slot, this.ConstantBuffer);

            this.Context.IA.SetVertexBuffer(component.Model.Vertices);
            this.Context.IA.SetIndexBuffer(component.Model.Indices);

            for (var i = 0; i < component.Model.Primitives.Length; i++)
            {
                var primitive = component.Model.Primitives[i];
                var material = component.Model.Materials[primitive.MaterialIndex];

                this.Context.PS.SetShaderResource(Geometry.Albedo, material.Albedo);
                this.Context.PS.SetShaderResource(Geometry.Normal, material.Normal);
                this.Context.PS.SetShaderResource(Geometry.Metalicness, material.Metalicness);
                this.Context.PS.SetShaderResource(Geometry.Roughness, material.Roughness);
                this.Context.PS.SetShaderResource(Geometry.AmbientOcclusion, material.AmbientOcclusion);
                this.Context.DrawIndexed(primitive.IndexCount, primitive.IndexOffset, 0);
            }
        }
    }

    public void OnUnSet()
    {
        // TODO: is it really useful to do this asynchronously?
        using var commandList = this.Context.FinishCommandList();
        this.Device.ImmediateContext.ExecuteCommandList(commandList);
    }

    public void Dispose()
    {
        this.ConstantBuffer.Dispose();
        this.InputLayout.Dispose();
        this.PixelShader.Dispose();
        this.VertexShader.Dispose();
        this.Context.Dispose();
    }


    private static BoundingBox Transform(in BoundingBox box, in Matrix4x4 transform)
    {
        var size = box.Maximum - box.Minimum;
        var newCenter = Vector3.Transform(box.Center, transform);
        var oldEdge = size * 0.5f;

        var newEdge = new Vector3(
            Math.Abs(transform.M11) * oldEdge.X + Math.Abs(transform.M12) * oldEdge.Y + Math.Abs(transform.M13) * oldEdge.Z,
            Math.Abs(transform.M21) * oldEdge.X + Math.Abs(transform.M22) * oldEdge.Y + Math.Abs(transform.M23) * oldEdge.Z,
            Math.Abs(transform.M31) * oldEdge.X + Math.Abs(transform.M32) * oldEdge.Y + Math.Abs(transform.M33) * oldEdge.Z
        );

        return new(newCenter - newEdge, newCenter + newEdge);
    }
}
