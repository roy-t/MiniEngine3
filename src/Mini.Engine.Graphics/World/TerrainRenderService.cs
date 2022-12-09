using Mini.Engine.Configuration;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.ECS.Components;
using Mini.Engine.Graphics.Models;
using Mini.Engine.Graphics.Transforms;

namespace Mini.Engine.Graphics.World;

[Service]
public sealed class TerrainRenderService
{
    private readonly IComponentContainer<TransformComponent> Transforms;
    private readonly IComponentContainer<TerrainComponent> Terrain;

    public TerrainRenderService(IComponentContainer<TransformComponent> transforms, IComponentContainer<TerrainComponent> terrain)
    {
        this.Transforms = transforms;
        this.Terrain = terrain;
    }

    public void RenderAllTerrain(DeviceContext context, in Frustum viewVolume, IMeshRenderServiceCallBack callBack)
    {
        var iterator = this.Terrain.IterateAll();        
        while (iterator.MoveNext())
        {
            ref var terrain = ref iterator.Current;
            ref var transform = ref this.Transforms[terrain.Entity];            
            RenderTerrain(context, in terrain, in transform, in viewVolume, callBack);
        }
    }

    public static void RenderTerrain(DeviceContext context, in TerrainComponent terrainComponent, in TransformComponent transformComponent, in Frustum viewVolume, IMeshRenderServiceCallBack callBack)
    {
        MeshRenderService.RenderMesh(context, terrainComponent.Mesh, in transformComponent, in viewVolume, callBack);       
    }
}
