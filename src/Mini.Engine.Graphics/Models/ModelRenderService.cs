using Mini.Engine.Configuration;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Resources.Models;
using Mini.Engine.ECS.Components;
using Mini.Engine.Graphics.Transforms;

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

    public ModelRenderService(IComponentContainer<TransformComponent> transforms, IComponentContainer<ModelComponent> models)
    {
        this.Transforms = transforms;
        this.Models = models;
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
}
