﻿using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.Content.Shaders;
using Mini.Engine.Content.Shaders.FlatShader;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.ECS.Generators.Shared;
using Mini.Engine.ECS.Systems;
using Mini.Engine.Graphics.Transforms;
using Vortice.Direct3D;

namespace Mini.Engine.Graphics.Models;

[Service]
public partial class ModelSystem : ISystem
{
    private readonly Device Device;
    private readonly DeferredDeviceContext Context;
    private readonly FrameService FrameService;
    private readonly FlatShaderVs VertexShader;
    private readonly FlatShaderPs PixelShader;
    private readonly InputLayout InputLayout;
    private readonly ConstantBuffer<CBuffer0> ConstantBuffer;


    public ModelSystem(Device device, FrameService frameService, ContentManager content)
    {
        this.Device = device;
        this.Context = device.CreateDeferredContextFor<ModelSystem>();
        this.FrameService = frameService;
        this.VertexShader = content.LoadFlatShaderVs();
        this.PixelShader = content.LoadFlatShaderPs();
        this.InputLayout = this.VertexShader.CreateInputLayout(device, ModelVertex.Elements);
        this.ConstantBuffer = new ConstantBuffer<CBuffer0>(device, "constants_modelsystem");
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
        this.Context.PS.SetSampler(FlatShader.TextureSampler, this.Device.SamplerStates.AnisotropicWrap);

        this.Context.OM.SetRenderTargets(this.FrameService.GBuffer.DepthStencilBuffer, this.FrameService.GBuffer.Albedo, this.FrameService.GBuffer.Material, this.FrameService.GBuffer.Depth, this.FrameService.GBuffer.Normal);

        this.Context.OM.SetBlendState(this.Device.BlendStates.Opaque);
        this.Context.OM.SetDepthStencilState(this.Device.DepthStencilStates.Default);
    }

    [Process(Query = ProcessQuery.All)]
    public void DrawModel(ModelComponent component, TransformComponent transform)
    {
        var world = transform.AsMatrix();
        var cBuffer = new CBuffer0()
        {
            WorldViewProjection = world * this.FrameService.Camera.ViewProjection,
            World = world,
            CameraPosition = this.FrameService.Camera.Transform.Position
        };
        this.ConstantBuffer.MapData(this.Context, cBuffer);

        this.Context.IA.SetVertexBuffer(component.Model.Vertices);
        this.Context.IA.SetIndexBuffer(component.Model.Indices);

        this.Context.VS.SetConstantBuffer(CBuffer0.Slot, this.ConstantBuffer);

        for (var i = 0; i < component.Model.Primitives.Length; i++)
        {
            var primitive = component.Model.Primitives[i];
            var material = component.Model.Materials[primitive.MaterialIndex];

            this.Context.PS.SetShaderResource(FlatShader.Albedo, material.Albedo);
            this.Context.PS.SetShaderResource(FlatShader.Normal, material.Normal);
            this.Context.PS.SetShaderResource(FlatShader.Metalicness, material.Metalicness);
            this.Context.PS.SetShaderResource(FlatShader.Roughness, material.Roughness);
            this.Context.PS.SetShaderResource(FlatShader.AmbientOcclusion, material.AmbientOcclusion);
            this.Context.DrawIndexed(primitive.IndexCount, primitive.IndexOffset, 0);
        }
    }

    public void OnUnSet()
    {
        // TODO: is it really useful to do this asynchronously?
        using var commandList = this.Context.FinishCommandList();
        this.Device.ImmediateContext.ExecuteCommandList(commandList);
    }
}