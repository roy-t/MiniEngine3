using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;

using GrassInstanceData = Mini.Engine.Content.Shaders.Generated.Grass.InstanceData;

namespace Mini.Engine.Graphics.Vegetation;

public struct GrassComponent : IComponent
{
    // TODO: maybe create an IVertexBuffer interface that inherits from IDeviceResource
    // and remove IDeviceResource from DeviceBuffer
    public ILifetime<StructuredBuffer<GrassInstanceData>> InstanceBuffer;
    public ILifetime<ITexture> Texture;
    public int Instances;

    public Entity Entity { get; set; }
    public LifeCycle LifeCycle { get; set; }
}
