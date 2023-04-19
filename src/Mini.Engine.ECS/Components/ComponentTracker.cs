using Mini.Engine.Configuration;

namespace Mini.Engine.ECS.Components;

public readonly record struct ComponentBit(ulong Bit);

[Service]
public sealed class ComponentTracker
{
    private readonly Dictionary<Entity, ComponentBit> EntityComponents;
    private const ulong BIT_MIN = 0b_0000000000000000000000000000000000000000000000000000000000000001UL;
    private const ulong BIT_Max = 0b_1000000000000000000000000000000000000000000000000000000000000000UL;

    private ulong bit;

    public ComponentTracker()
    {
        this.EntityComponents = new Dictionary<Entity, ComponentBit>();
    }

    public ComponentBit GetBit()        
    {
        if (this.bit < BIT_MIN)
        {
            this.bit = BIT_MIN;
        }
        else if(this.bit < BIT_Max)
        {
            this.bit <<= 1;
        }
        else
        {
            throw new Exception("More than 64 component types, update range of ComponentBit");
        }

        return new ComponentBit(this.bit);
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

        this.EntityComponents[entity] = new ComponentBit(components.Bit & ~component.Bit);
    }
}