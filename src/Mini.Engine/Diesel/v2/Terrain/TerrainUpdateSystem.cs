using Mini.Engine.Configuration;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.Graphics.Primitives;
using Mini.Engine.Graphics;
using Mini.Engine.DirectX;
using Mini.Engine.ECS.Systems;
using Mini.Engine.ECS.Components;
using Mini.Engine.ECS;
using Mini.Engine.Diesel.Trains;
using Mini.Engine.Content;

namespace Mini.Engine.Diesel.v2.Terrain;

[Service]
public sealed class TerrainUpdateSystem : IDisposable
{

    private readonly DeferredDeviceContext Context;
    private readonly ImmediateDeviceContext CompletionContext;
    private readonly TerrainGrid Terrain;

    public TerrainUpdateSystem(Device device, ContentManager content, ECSAdministrator administrator, IComponentContainer<InstancesComponent> instances)
    {
        this.Context = device.CreateDeferredContextFor<TerrainUpdateSystem>();
        this.CompletionContext = device.ImmediateContext;

        var bogiePrimitive = TrainCars.BuildBogie(device, content, "bogie");
        var bogieBounds = device.Resources.Get(bogiePrimitive).Bounds;
        var mesh = TrainCars.BuildFlatCar(device, content, "flat_car", in bogieBounds);

        var entity = PrimitiveUtilities.CreateComponents(device, administrator, mesh, 1000, 1.0f);

        this.Terrain = new TerrainGrid(instances, entity, 10, 10, 10, 5);      
    }

    public Task<ICompletable> Update()
    {
        return Task.Run(() =>
        {
            // TODO: REMOVE!
            return CompletableCommandList.Create(this.CompletionContext, this.Context.FinishCommandList());
        });
    }

    public void Dispose()
    {
        this.Context.Dispose();
    }

}
