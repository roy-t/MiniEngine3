using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;
using Mini.Engine.ECS.Systems;

namespace Mini.Engine.Graphics.Primitives;

[Service]
public sealed class InstancesSystem : IDisposable
{
    private record WorkItem(Entity Entity, List<Matrix4x4> InstanceList);

    private readonly DeferredDeviceContext Context;
    private readonly ImmediateDeviceContext CompletionContext;    
    private readonly IComponentContainer<InstancesComponent> Instances;

    private readonly Queue<WorkItem> Queue;

    public InstancesSystem(Device device, IComponentContainer<InstancesComponent> instances)
    {        
        this.Instances = instances;        
        this.Context = device.CreateDeferredContextFor<InstancesSystem>();
        this.CompletionContext = device.ImmediateContext;

        this.Queue = new Queue<WorkItem>();
    }

    public void QueueUpdate(Entity entity, List<Matrix4x4> instanceList)
    {
        this.Queue.Enqueue(new WorkItem(entity, instanceList));
    }

    public Task<ICompletable> UpdateInstances()
    {
        return Task.Run(() =>
        {
            while(this.Queue.Count > 0)
            {
                var item = this.Queue.Dequeue();
                ref var component = ref this.Instances[item.Entity];
                Instancing.MapInstanceData(this.Context, ref component.Value, item.InstanceList);
            }            
            return CompletableCommandList.Create(this.CompletionContext, this.Context.FinishCommandList());
        });
    }

    public void Dispose()
    {
        this.Context.Dispose();
    }
}
