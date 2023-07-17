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
    }

    public Entity Entity { get; }
    public string Name { get; }
    public ICurve Curve { get; }
}
