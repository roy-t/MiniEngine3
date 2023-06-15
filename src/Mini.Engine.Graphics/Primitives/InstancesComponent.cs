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
    public int InstanceCount;

    public void Init(Device device, string name, params Matrix4x4[] instances)
    {
        var buffer = new StructuredBuffer<Matrix4x4>(device, name);
        buffer.MapData(device.ImmediateContext, instances);
        var view = buffer.CreateShaderResourceView();

        this.InstanceBuffer = device.Resources.Add(buffer);
        this.InstanceBufferView = device.Resources.Add(view);
        this.InstanceCount = instances.Length;
    }
}
