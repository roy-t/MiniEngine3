using Mini.Engine.Configuration;

namespace Mini.Engine.ECS.Experimental;

public readonly record struct ComponentBit(ulong Bit);

[Service]
public sealed class ComponentTracker
{
    private readonly Dictionary<Guid, ComponentBit> Bits;

    public ComponentTracker(IEnumerable<IComponentContainer> containers)
    {
        this.Bits = new Dictionary<Guid, ComponentBit>();

        var bit = 0b_0000000000000000000000000000000000000000000000000000000000000001UL;

        foreach (var container in containers)
        {
            this.Bits.Add(container.ComponentType.GUID, new ComponentBit() { Bit = bit });
            bit <<= 1;
        }
    }

    public ComponentBit GetBit<T>()
        where T : IComponent
    {
        return this.Bits[typeof(T).GUID];
    }

    public static bool HasComponent(Entity entity, ComponentBit component)
    {
        return (entity.Components.Bit & component.Bit) > 0;
    }

    public static void SetComponent(ref Entity entity, ComponentBit component)
    {
        entity.Components = new ComponentBit(entity.Components.Bit | component.Bit);
    }

    public static void UnsetComponent(ref Entity entity, ComponentBit component)
    {
        entity.Components = new ComponentBit(entity.Components.Bit & (~component.Bit));
    }
}