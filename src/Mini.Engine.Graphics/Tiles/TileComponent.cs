using Mini.Engine.ECS.Components;
using Mini.Engine.ECS;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Resources.Models;

using TileInstanceData = Mini.Engine.Content.Shaders.Generated.Tiles.InstanceData;

namespace Mini.Engine.Graphics.Tiles;
public struct TileComponent : IComponent
{
    public ILifetime<StructuredBuffer<TileInstanceData>> InstanceBuffer;    
    public ILifetime<IMaterial> Material;
    public uint Columns;
    public uint Rows;

    public Entity Entity { get; set; }
    public LifeCycle LifeCycle { get; set; }
}
