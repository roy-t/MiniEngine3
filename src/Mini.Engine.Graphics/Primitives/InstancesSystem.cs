using System.Runtime.InteropServices;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.ECS.Components;
using Mini.Engine.ECS.Systems;

namespace Mini.Engine.Graphics.Primitives;

[Service]
public sealed class InstancesSystem : IDisposable
{
    private const int BufferCapacityIncrement = 100;

    private readonly DeferredDeviceContext Context;
    private readonly ImmediateDeviceContext CompletionContext;    
    private readonly IComponentContainer<InstancesComponent> Instances;

    public InstancesSystem(Device device, IComponentContainer<InstancesComponent> instances)
    {        
        this.Instances = instances;        
        this.Context = device.CreateDeferredContextFor<InstancesSystem>();
        this.CompletionContext = device.ImmediateContext;
    }

    public Task<ICompletable> UpdateInstances()
    {
        return Task.Run(() =>
        {
            foreach (ref var component in this.Instances.IterateNew())
            {
                this.MapInstanceData(in component.Value);
            }

            foreach (ref var component in this.Instances.IterateChanged())
            {
                this.MapInstanceData(in component.Value);
            }

            return CompletableCommandList.Create(this.CompletionContext, this.Context.FinishCommandList());
        });
    }

    private void MapInstanceData(in InstancesComponent instance)
    {
        var buffer = this.Context.Resources.Get(instance.InstanceBuffer);
        var count = instance.InstanceList.Count;

        buffer.EnsureCapacity(count, BufferCapacityIncrement);

        if (count > 0)
        {
            buffer.MapData(this.Context, CollectionsMarshal.AsSpan(instance.InstanceList));
        }
    }

    public void Dispose()
    {
        this.Context.Dispose();
    }
}
