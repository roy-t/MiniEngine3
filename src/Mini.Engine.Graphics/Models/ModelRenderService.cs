using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Contexts.States;
using Mini.Engine.DirectX.Resources.Models;
using Mini.Engine.ECS.Components;
using Mini.Engine.Graphics.Transforms;
using Vortice.Direct3D;
using ShadowMap = Mini.Engine.Content.Shaders.Generated.ShadowMap;

namespace Mini.Engine.Graphics.Models;

public interface IModelRenderServiceCallBack
{
    void RenderModelCallback(in TransformComponent transform);
    void RenderPrimitiveCallback(IModel model, Primitive primitive);
}

[Service]
public sealed class ModelRenderService
{
    private readonly IComponentContainer<TransformComponent> Transforms;
    private readonly IComponentContainer<ModelComponent> Models;

    private readonly ShadowMap ShadowMapShader;
    private readonly ShadowMap.User ShadowMapUser;
    private readonly InputLayout ShadowMapInputLayout;

    private readonly RasterizerState CullNoneNoDepthClip;
    private readonly DepthStencilState Default;
    private readonly BlendState Opaque;
    private readonly SamplerState AnisotropicWrap;

    public ModelRenderService(Device device, ShadowMap shadowMapShader, IComponentContainer<TransformComponent> transforms, IComponentContainer<ModelComponent> models)
    {
        this.Transforms = transforms;
        this.Models = models;

        this.ShadowMapShader = shadowMapShader;
        this.ShadowMapUser = shadowMapShader.CreateUserFor<ModelRenderService>();
        this.ShadowMapInputLayout = this.ShadowMapShader.CreateInputLayoutForVs(ModelVertex.Elements);

        this.CullNoneNoDepthClip = device.RasterizerStates.CullNoneNoDepthClip;
        this.Default = device.DepthStencilStates.Default;
        this.AnisotropicWrap = device.SamplerStates.AnisotropicWrap;
        this.Opaque = device.BlendStates.Opaque;
    }

    public void RenderAllModels(DeviceContext context, in Frustum viewVolume, IModelRenderServiceCallBack callBack)
    {
        var iterator = this.Models.IterateAll();
        while (iterator.MoveNext())
        {
            ref var model = ref iterator.Current;
            ref var transform = ref this.Transforms[model.Entity];            
            RenderModel(context, in model, in transform, viewVolume, callBack);
        }
    }

    public static void RenderModel(DeviceContext context, in ModelComponent modelComponent, in TransformComponent transformComponent, in Frustum viewVolume, IModelRenderServiceCallBack callBack)
    {
        var model = context.Resources.Get(modelComponent.Model);
        var world = transformComponent.Current.GetMatrix();
        var bounds = model.Bounds.Transform(world);

        if (viewVolume.ContainsOrIntersects(bounds))
        {
            callBack.RenderModelCallback(in transformComponent);
            
            context.IA.SetVertexBuffer(model.Vertices);
            context.IA.SetIndexBuffer(model.Indices);

            for (var i = 0; i < model.Primitives.Count; i++)
            {
                var primitive = model.Primitives[i];
                bounds = primitive.Bounds.Transform(world);

                if (viewVolume.ContainsOrIntersects(bounds))
                {
                    callBack.RenderPrimitiveCallback(model, primitive);
                    context.DrawIndexed(primitive.IndexCount, primitive.IndexOffset, 0);
                }
            }
        }
    }

    /////////////////////////

    public void SetupModelDepthRender(DeviceContext context, int x, int y, int width, int height)
    {
        context.Setup(this.ShadowMapInputLayout, PrimitiveTopology.TriangleList, this.ShadowMapShader.Vs, this.CullNoneNoDepthClip, x, y, width, height, this.ShadowMapShader.Ps, this.Opaque, this.Default);
        context.VS.SetConstantBuffer(ShadowMap.ConstantsSlot, this.ShadowMapUser.ConstantsBuffer);
        context.PS.SetSampler(ShadowMap.TextureSampler, this.AnisotropicWrap);
    }

    public void RenderModelDepth(DeviceContext context, in ModelComponent modelComponent, in TransformComponent transformComponent, in Frustum viewVolume)
    {
        var model = context.Resources.Get(modelComponent.Model);
        var world = transformComponent.Current.GetMatrix();
        var bounds = model.Bounds.Transform(world);

        if (viewVolume.ContainsOrIntersects(bounds))
        {
            // TODO: pass camera and fill in consants from callbacks!
            this.ShadowMapUser.MapConstants(context, world * this.ViewProjection); 

            context.IA.SetVertexBuffer(model.Vertices);
            context.IA.SetIndexBuffer(model.Indices);

            for (var i = 0; i < model.Primitives.Count; i++)
            {
                var primitive = model.Primitives[i];
                bounds = primitive.Bounds.Transform(world);

                if (viewVolume.ContainsOrIntersects(bounds))
                {
                    callBack.RenderPrimitiveCallback(model, primitive);
                    context.DrawIndexed(primitive.IndexCount, primitive.IndexOffset, 0);
                }
            }
        }
    }
}
