using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.ECS.Components;
using Mini.Engine.Graphics.Transforms;

namespace Mini.Engine.Graphics.Models;

public interface IRenderServiceCallBack
{
    void SetConstants(Matrix4x4 worldViewProjection, Matrix4x4 world);
    void SetMaterial(IMaterial material);
}

[Service]
public sealed class RenderService
{
    private readonly IComponentContainer<ModelComponent> Models;
    private readonly IComponentContainer<TransformComponent> Transforms;

    public RenderService(IComponentContainer<ModelComponent> models, IComponentContainer<TransformComponent> transforms)
    {
        this.Models = models;
        this.Transforms = transforms;
    }

    public void DrawAllModels(IRenderServiceCallBack callback, DeviceContext context, Matrix4x4 viewProjection)
    {
        foreach (var model in this.Models.GetAllItems())
        {
            var transform = this.Transforms[model.Entity];
            this.DrawModel(callback, context, viewProjection, model.Model, transform.Transform);
        }
    }

    public void DrawModel(IRenderServiceCallBack callback, DeviceContext context, Matrix4x4 viewProjection, IModel model, Transform transform)
    {
        var world = transform.Matrix;
        var bounds = model.Bounds.Transform(world);
        var frustum = new Frustum(viewProjection);

        if (frustum.ContainsOrIntersects(bounds))
        {
            callback.SetConstants(world * viewProjection, world);

            context.IA.SetVertexBuffer(model.Vertices);
            context.IA.SetIndexBuffer(model.Indices);

            for (var i = 0; i < model.Primitives.Length; i++)
            {
                var primitive = model.Primitives[i];

                bounds = primitive.Bounds.Transform(world);

                if (frustum.ContainsOrIntersects(bounds))
                {
                    callback.SetMaterial(model.Materials[primitive.MaterialIndex]);
                    context.DrawIndexed(primitive.IndexCount, primitive.IndexOffset, 0);
                }
            }
        }

    }
}
