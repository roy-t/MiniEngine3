using System.Numerics;
using Mini.Engine.ECS;
using Mini.Engine.Modelling.Curves;

namespace Mini.Engine.Diesel.Tracks;

public sealed class TrackPiece
{
    public TrackPiece(Entity entity, string name, ICurve curve)
    {
        this.Entity = entity;
        this.Name = name;
        this.Curve = curve;
        this.Instances = new List<Matrix4x4>();
    }

    public Entity Entity { get; }
    public string Name { get; }
    public ICurve Curve { get; }
    public List<Matrix4x4> Instances { get; }
}
