using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Resources.Models;
using Mini.Engine.Graphics.Transforms;

namespace Mini.Engine.Graphics.Models;

public interface IMeshRenderServiceCallBack
{
    void RenderMesh(in TransformComponent transform);    
}

public static class MeshRenderService
{
    public static void RenderMesh(DeviceContext context, ILifetime<IMesh> mesh, in TransformComponent transformComponent, in Frustum viewVolume, IMeshRenderServiceCallBack callBack)
    {
        var cMesh = context.Resources.Get(mesh);
        var world = transformComponent.Current.GetMatrix();
        var bounds = cMesh.Bounds.Transform(world);

        if (viewVolume.ContainsOrIntersects(bounds))
        {
            callBack.RenderMesh(in transformComponent);

            context.IA.SetVertexBuffer(cMesh.Vertices);
            context.IA.SetIndexBuffer(cMesh.Indices);

            context.DrawIndexed(cMesh.Indices.Length, 0, 0);
        }
    }
}
