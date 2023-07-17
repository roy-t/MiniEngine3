using System.Numerics;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.ECS.Components;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX;

namespace Mini.Engine.Graphics.Primitives;

public struct InstancesComponent : IComponent
{
    public ILifetime<StructuredBuffer<Matrix4x4>> InstanceBuffer;
    public ILifetime<ShaderResourceView<Matrix4x4>> InstanceBufferView;    

    // TODO: is it wrong to have a reference type here?
    public List<Matrix4x4> InstanceList;

    public void Init(Device device, string name, int capacity = 0)
    {
        var buffer = new StructuredBuffer<Matrix4x4>(device, name, capacity);
        var view = buffer.CreateShaderResourceView();

        this.InstanceBuffer = device.Resources.Add(buffer);
        this.InstanceBufferView = device.Resources.Add(view);

        this.InstanceList = new List<Matrix4x4>(capacity);
    }
}
