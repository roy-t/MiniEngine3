using System.Numerics;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.ECS.Components;
using Mini.Engine.Core.Lifetime;

namespace Mini.Engine.Graphics.Primitives;
public struct InstancesComponent : IComponent
{
    public ILifetime<StructuredBuffer<Matrix4x4>> InstanceBuffer;
    public int InstanceCount;
}
