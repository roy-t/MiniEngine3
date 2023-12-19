using Mini.Engine.Configuration;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.Graphics.Primitives;
using Mini.Engine.Graphics;
using Mini.Engine.DirectX;
using Mini.Engine.ECS.Systems;
using Mini.Engine.ECS;

namespace Mini.Engine.Diesel.v2.Terrain;

[Service]
public sealed class TerrainUpdateSystem : IDisposable
{

    private readonly DeferredDeviceContext Context;
    private readonly ImmediateDeviceContext CompletionContext;
    private readonly TerrainGrid Terrain;
    private readonly InstancesSystem Instances;

    public TerrainUpdateSystem(Device device, ECSAdministrator administrator, InstancesSystem instances)
    {
        this.Context = device.CreateDeferredContextFor<TerrainUpdateSystem>();
        this.CompletionContext = device.ImmediateContext;

        var mesh = PrimitiveUtilities.BuildPlane(device, out var _);

        var entity = PrimitiveUtilities.CreateComponents(device, administrator, mesh, 1000, 1.0f);

        // With more than 1000 cells, drawing stops working, might be some undocumented limit for DrawIndexedInstanced?
        this.Instances = instances;
        this.Terrain = new TerrainGrid(instances, entity, 30, 30, 10, 5);      
    }

    public Task<ICompletable> Update()
    {
        return Task.Run(() =>
        {
            foreach(var entity in this.Terrain.Dirty)
            {
                var instances = this.Terrain.LookUp[entity];
                this.Instances.QueueUpdate(entity, instances);
            }
            return CompletableCommandList.Create(this.CompletionContext, this.Context.FinishCommandList());
        });
    }

    public void Dispose()
    {
        this.Context.Dispose();
    }

}
