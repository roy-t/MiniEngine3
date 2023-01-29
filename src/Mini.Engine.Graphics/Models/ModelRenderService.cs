using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Contexts.States;
using Mini.Engine.DirectX.Resources.Models;
using Mini.Engine.ECS.Components;
using Mini.Engine.Graphics.Cameras;
using Mini.Engine.Graphics.Transforms;
using Vortice.Direct3D;

using Geometry = Mini.Engine.Content.Shaders.Generated.Geometry;
using ShadowMap = Mini.Engine.Content.Shaders.Generated.ShadowMap;

namespace Mini.Engine.Graphics.Models;

[Service]
public sealed class ModelRenderService
{
    private readonly IComponentContainer<TransformComponent> Transforms;
    private readonly IComponentContainer<ModelComponent> Models;

    private readonly Geometry Shader;
    private readonly Geometry.User User;
    private readonly InputLayout InputLayout;

    private readonly ShadowMap ShadowMapShader;
    private readonly ShadowMap.User ShadowMapUser;
    private readonly InputLayout ShadowMapInputLayout;

    private readonly RasterizerState CullCounterClockwise;
    private readonly RasterizerState CullNoneNoDepthClip;
    private readonly DepthStencilState Default;
    private readonly DepthStencilState ReverseZ;
    private readonly BlendState Opaque;
    private readonly SamplerState AnisotropicWrap;

    public ModelRenderService(Device device, Geometry shader, ShadowMap shadowMapShader, IComponentContainer<TransformComponent> transforms, IComponentContainer<ModelComponent> models)
    {
        this.Transforms = transforms;
        this.Models = models;

        this.Shader = shader;
        this.User = shader.CreateUserFor<ModelRenderService>();
        this.InputLayout = shader.CreateInputLayoutForVs(ModelVertex.Elements);

        this.ShadowMapShader = shadowMapShader;
        this.ShadowMapUser = shadowMapShader.CreateUserFor<ModelRenderService>();
        this.ShadowMapInputLayout = this.ShadowMapShader.CreateInputLayoutForVs(ModelVertex.Elements);

        this.CullCounterClockwise = device.RasterizerStates.CullCounterClockwise;
        this.CullNoneNoDepthClip = device.RasterizerStates.CullNoneNoDepthClip;
        this.Default = device.DepthStencilStates.Default;
        this.ReverseZ = device.DepthStencilStates.ReverseZ;
        this.AnisotropicWrap = device.SamplerStates.AnisotropicWrap;
        this.Opaque = device.BlendStates.Opaque;
    }

    /// <summary>
    /// Configures everything for rendering tiles, except for the output (render target)
    /// </summary>    
    public void SetupModelRender(DeviceContext context, int x, int y, int width, int height)
    {
        context.Setup(this.InputLayout, PrimitiveTopology.TriangleList, this.Shader.Vs, this.CullCounterClockwise, x, y, width, height, this.Shader.Ps, this.Opaque, this.ReverseZ);

        context.VS.SetConstantBuffer(Geometry.ConstantsSlot, this.User.ConstantsBuffer);
        context.PS.SetConstantBuffer(Geometry.ConstantsSlot, this.User.ConstantsBuffer);
        context.PS.SetSampler(Geometry.TextureSampler, this.AnisotropicWrap);
    }

    public void RenderModel(DeviceContext context, in ModelComponent modelComponent, in TransformComponent transformComponent, in CameraComponent cameraComponent, in TransformComponent cameraTransformComponent)
    {
        var viewProjection = cameraComponent.Camera.GetInfiniteReversedZViewProjection(in cameraTransformComponent.Current, cameraComponent.Jitter);
        var viewVolume = new Frustum(viewProjection);

        var model = context.Resources.Get(modelComponent.Model);
        var world = transformComponent.Current.GetMatrix();
        var bounds = model.Bounds.Transform(world);

        if (viewVolume.ContainsOrIntersects(bounds))
        {
            var previousViewProjection = cameraComponent.Camera.GetInfiniteReversedZViewProjection(in cameraTransformComponent.Previous, cameraComponent.PreviousJitter);
            var previousWorld = transformComponent.Previous.GetMatrix();
            var previousWorldViewProjection = previousWorld * previousViewProjection;
            var worldViewProjection = world * viewProjection;
            this.User.MapConstants(context, previousWorldViewProjection, worldViewProjection, world, cameraTransformComponent.Current.GetPosition(), cameraComponent.PreviousJitter, cameraComponent.Jitter);

            context.IA.SetVertexBuffer(model.Vertices);
            context.IA.SetIndexBuffer(model.Indices);

            for (var i = 0; i < model.Primitives.Count; i++)
            {
                var primitive = model.Primitives[i];
                bounds = primitive.Bounds.Transform(world);

                if (viewVolume.ContainsOrIntersects(bounds))
                {
                    var material = context.Resources.Get(model.Materials[primitive.MaterialIndex]);

                    context.PS.SetShaderResource(Geometry.Albedo, material.Albedo);
                    context.PS.SetShaderResource(Geometry.Normal, material.Normal);
                    context.PS.SetShaderResource(Geometry.Metalicness, material.Metalicness);
                    context.PS.SetShaderResource(Geometry.Roughness, material.Roughness);
                    context.PS.SetShaderResource(Geometry.AmbientOcclusion, material.AmbientOcclusion);

                    context.DrawIndexed(primitive.IndexCount, primitive.IndexOffset, 0);
                }
            }
        }
    }

    public void SetupModelDepthRender(DeviceContext context, int x, int y, int width, int height)
    {
        context.Setup(this.ShadowMapInputLayout, PrimitiveTopology.TriangleList, this.ShadowMapShader.Vs, this.CullNoneNoDepthClip, x, y, width, height, this.ShadowMapShader.Ps, this.Opaque, this.Default);
        context.VS.SetConstantBuffer(ShadowMap.ConstantsSlot, this.ShadowMapUser.ConstantsBuffer);
        context.PS.SetSampler(ShadowMap.TextureSampler, this.AnisotropicWrap);
    }

    public void RenderModelDepth(DeviceContext context, in ModelComponent modelComponent, in TransformComponent transformComponent, in Frustum viewVolume, in Matrix4x4 viewProjection)
    {
        var model = context.Resources.Get(modelComponent.Model);
        var world = transformComponent.Current.GetMatrix();
        var bounds = model.Bounds.Transform(world);

        if (viewVolume.ContainsOrIntersects(bounds))
        {
            this.ShadowMapUser.MapConstants(context, world * viewProjection);

            context.IA.SetVertexBuffer(model.Vertices);
            context.IA.SetIndexBuffer(model.Indices);

            for (var i = 0; i < model.Primitives.Count; i++)
            {
                var primitive = model.Primitives[i];
                bounds = primitive.Bounds.Transform(world);

                if (viewVolume.ContainsOrIntersects(bounds))
                {
                    var material = context.Resources.Get(model.Materials[primitive.MaterialIndex]);
                    context.PS.SetShaderResource(ShadowMap.Albedo, material.Albedo);
                    context.DrawIndexed(primitive.IndexCount, primitive.IndexOffset, 0);
                }
            }
        }
    }

    public void SetupAndRenderAllModelDepths(DeviceContext context, int x, int y, int width, int height, in Frustum viewVolume, in Matrix4x4 viewProjection)
    {
        this.SetupModelDepthRender(context, x, y, width, height);

        var iterator = this.Models.IterateAll();
        while (iterator.MoveNext())
        {
            ref var model = ref iterator.Current;
            ref var transform = ref this.Transforms[model.Entity];
            this.RenderModelDepth(context, in model, in transform, in viewVolume, in viewProjection);
        }
    }
}
