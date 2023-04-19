using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Resources.Models;
using Mini.Engine.ECS.Components;

using HexagonInstanceData = Mini.Engine.Content.Shaders.Generated.Hexagon.InstanceData;

namespace Mini.Engine.Graphics.Hexagons;
public struct HexagonTerrainComponent : IComponent
{
    public ILifetime<StructuredBuffer<HexagonInstanceData>> InstanceBuffer;
    public ILifetime<IMaterial> Material;
    public int Instances;    
}
