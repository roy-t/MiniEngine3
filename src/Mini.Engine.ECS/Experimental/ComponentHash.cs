using Mini.Engine.Configuration;

namespace Mini.Engine.ECS.Experimental;

public readonly record struct ComponentBit(ulong Bit);

[Service]
public sealed class ComponentTracker
{
    private readonly Dictionary<Guid, ComponentBit> Bits;
    private readonly Dictionary<Entity, ComponentBit> EntityComponents;

    public ComponentTracker(ComponentCatalog components)
    {
        this.Bits = new Dictionary<Guid, ComponentBit>();
        this.EntityComponents = new Dictionary<Entity, ComponentBit>();

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

    public bool HasComponent(Entity entity, ComponentBit component)
    {
        if (this.EntityComponents.ContainsKey(entity))
        {
            var components = this.EntityComponents[entity];
            return (components.Bit & component.Bit) > 0;
        }

        return false;
    }

    public void SetComponent(Entity entity, ComponentBit component)
    {
        var components = new ComponentBit();
        if (this.EntityComponents.ContainsKey(entity))
        {
            components = this.EntityComponents[entity];
        }

        this.EntityComponents[entity] = new ComponentBit(components.Bit | component.Bit);
    }

    public void UnsetComponent(Entity entity, ComponentBit component)
    {
        var components = new ComponentBit();
        if (this.EntityComponents.ContainsKey(entity))
        {
            components = this.EntityComponents[entity];
        }

        this.EntityComponents[entity] = new ComponentBit(components.Bit & (~component.Bit));        
    }
}