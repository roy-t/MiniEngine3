using System.Numerics;
using System.Runtime.InteropServices;
using Mini.Engine.DirectX.Contexts;

namespace Mini.Engine.Graphics.Primitives;
public static class Instancing
{
    private const int BufferCapacityIncrement = 100;

    public static void MapInstanceData(DeviceContext context, ref InstancesComponent instance, List<Matrix4x4> instanceList, int bufferCapacityIncrement = BufferCapacityIncrement)
    {
        var count = instanceList.Count;
        instance.Count = count;

        if (count > 0)
        {
            var buffer = context.Resources.Get(instance.InstanceBuffer);
            buffer.EnsureCapacity(count, bufferCapacityIncrement);
            buffer.MapData(context, CollectionsMarshal.AsSpan(instanceList));            
        }
    }
}
