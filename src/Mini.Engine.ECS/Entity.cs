using System;
using System.Diagnostics.CodeAnalysis;

namespace Mini.Engine.ECS
{
    public readonly struct Entity : IEquatable, IEquatable<Entity>, IComparable, IComparable<Entity>
    {
        public readonly int Id;

        public Entity(int id)
        {
            this.Id = id;
        }

        public override string ToString()
        {
            return this.Id.ToString();
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        public int CompareTo(Entity other)
        {
            return this.Id.CompareTo(other.Id);
        }

        public int CompareTo(object? obj)
        {
            if (obj is Entity entity)
            {
                return this.CompareTo(entity);
            }
            return -1;
        }

        public bool Equals(Entity other)
        {
            return this.Id == other.Id;
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
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
}
