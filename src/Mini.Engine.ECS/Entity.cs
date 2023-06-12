using System.Diagnostics.CodeAnalysis;
using Mini.Engine.ECS.Components;

namespace Mini.Engine.ECS;

public readonly struct Entity : IEquatable<Entity>, IComparable, IComparable<Entity>
{
    public readonly int Id;

    public Entity(int id)
    {
        this.Id = id;
    }

    public readonly bool HasComponent(IComponentContainer container)
    {
        return container.Contains(this);
    }

    public readonly bool HasComponents(params IComponentContainer[] containers)
    {
        for (var i = 0; i < containers.Length; i++)
        {
            if (!containers[i].Contains(this))
            {
                return false;
            }
        }

        return true;
    }

    public readonly override string ToString()
    {
        return this.Id.ToString();
    }

    public readonly override int GetHashCode()
    {
        return this.Id.GetHashCode();
    }

    public readonly int CompareTo(Entity other)
    {
        return this.Id.CompareTo(other.Id);
    }

    public readonly int CompareTo(object? obj)
    {
        if (obj is Entity entity)
        {
            return this.CompareTo(entity);
        }
        return -1;
    }

    public readonly bool Equals(Entity other)
    {
        return this.Id == other.Id;
    }

    public readonly override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is Entity entity && this.Equals(entity);
    }

    public static bool operator ==(Entity left, Entity right) => left.Equals(right);

    public static bool operator !=(Entity left, Entity right) => !(left == right);

    public static bool operator <(Entity left, Entity right) => left.CompareTo(right) < 0;

    public static bool operator <=(Entity left, Entity right) => left.CompareTo(right) <= 0;

    public static bool operator >(Entity left, Entity right) => left.CompareTo(right) > 0;

    public static bool operator >=(Entity left, Entity right) => left.CompareTo(right) >= 0;
}
