using System.Numerics;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;

namespace Mini.Engine.Graphics.Vegetation;

public struct GrassInstanceData
{
    public Vector3 Position;
}

public struct GrassComponent : IComponent
{
    // TODO: maybe create an IVertexBuffer interface that inherits from IDeviceResource
    // and remove IDeviceResource from DeviceBuffer
    public IResource<StructuredBuffer<GrassInstanceData>> InstanceBuffer;
    public int Instances;

    public Entity Entity { get; set; }
    public LifeCycle LifeCycle { get; set; }
}
