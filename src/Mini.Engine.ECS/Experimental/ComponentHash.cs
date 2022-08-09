using Mini.Engine.Configuration;

namespace Mini.Engine.ECS.Experimental;

public readonly record struct ComponentBit(ulong Bit);

[Service]
public sealed class ComponentTracker
{
    private readonly Dictionary<Guid, ComponentBit> Bits;
        
    public ComponentTracker(ComponentCatalog components)
    {
        this.Bits = new Dictionary<Guid, ComponentBit>();

        var bit = 0b_0000000000000000000000000000000000000000000000000000000000000001UL;

        if (components.Count > 64)
        {
            throw new Exception("More than 64 component types, update range of ComponentBit");
        }

        foreach (var type in components)
        {
            this.Bits.Add(type.GUID, new ComponentBit(bit));
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