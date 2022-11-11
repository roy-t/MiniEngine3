using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Resources.Models;
using Mini.Engine.ECS.Components;
using Mini.Engine.Graphics.Transforms;
using Mini.Engine.Graphics.World;

namespace Mini.Engine.Graphics.Models;

public interface IMeshRenderCallBack
{
    void SetConstants(Matrix4x4 worldViewProjection, Matrix4x4 world);
}

public interface IModelRenderCallBack : IMeshRenderCallBack
{
    void SetMaterial(IMaterial material);
}

[Service]
public sealed class RenderService
{    
    private readonly IComponentContainer<TransformComponent> Transforms;
    private readonly IComponentContainer<ModelComponent> Models;
    private readonly IComponentContainer<TerrainComponent> Terrain;

    public RenderService(IComponentContainer<TransformComponent> transforms, IComponentContainer<ModelComponent> models, IComponentContainer<TerrainComponent> terrain)
    {
        this.Transforms = transforms;
        this.Models = models;
        this.Terrain = terrain;
    }

    public void DrawAllModels(IModelRenderCallBack callback, DeviceContext context, Matrix4x4 viewProjection)
    {
        var viewVolume = new Frustum(viewProjection);

        var iterator = this.Models.IterateAll();
        while (iterator.MoveNext())
        {
            ref var component = ref iterator.Current;
            var transform = this.Transforms[component.Entity];
            var model = context.Resources.Get(component.Model);
            DrawModel(callback, context, viewVolume, viewProjection, model, transform.Transform);
        }
    }

    public void DrawAllTerrain(IMeshRenderCallBack callback, DeviceContext context, Matrix4x4 viewProjection)
    {
        var iterator = this.Terrain.IterateAll();
        var viewVolume = new Frustum(viewProjection);
        while (iterator.MoveNext())
        {
            ref var terrain = ref iterator.Current;
            var transform = this.Transforms[terrain.Entity];
            var mesh = context.Resources.Get(terrain.Mesh);
            DrawMesh(callback, context, viewVolume, viewProjection, mesh, transform.Transform);
        }
    }

    public static void DrawModel(IModelRenderCallBack callback, DeviceContext context, Frustum viewVolume, Matrix4x4 viewProjection, IModel model, Transform transform)
    {        
        var world = transform.GetMatrix();
        var bounds = model.Bounds.Transform(world);
        
        if (viewVolume.ContainsOrIntersects(bounds))
        {
            callback.SetConstants(world * viewProjection, world);

            context.IA.SetVertexBuffer(model.Vertices);
            context.IA.SetIndexBuffer(model.Indices);

            for (var i = 0; i < model.Primitives.Count; i++)
            {
                var primitive = model.Primitives[i];
                var material = context.Resources.Get(model.Materials[primitive.MaterialIndex]);

                bounds = primitive.Bounds.Transform(world);

                if (viewVolume.ContainsOrIntersects(bounds))
                {
                    callback.SetMaterial(material);
                    context.DrawIndexed(primitive.IndexCount, primitive.IndexOffset, 0);
                }
            }
        }
    }

    public static void DrawMesh(IMeshRenderCallBack callback, DeviceContext context, Frustum viewVolume, Matrix4x4 viewProjection, IMesh mesh, Transform transform)
    {
        var world = transform.GetMatrix();
        var bounds = mesh.Bounds.Transform(world);

        if (viewVolume.ContainsOrIntersects(bounds))
        {
            callback.SetConstants(world * viewProjection, world);

            context.IA.SetVertexBuffer(mesh.Vertices);
            context.IA.SetIndexBuffer(mesh.Indices);

            if (viewVolume.ContainsOrIntersects(bounds))
            {
                context.DrawIndexed(mesh.Indices.Length, 0, 0);
            }
        }
    }
}
