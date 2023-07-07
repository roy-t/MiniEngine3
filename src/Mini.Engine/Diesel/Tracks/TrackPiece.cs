using System.Numerics;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.ECS;
using Mini.Engine.Graphics.Primitives;
using Mini.Engine.Modelling.Curves;
using Vortice.Mathematics;

namespace Mini.Engine.Diesel.Tracks;

public readonly record struct TrackInstance(Matrix4x4 Transform, int Id);

public sealed class TrackPiece
{
    public TrackPiece(Entity entity, string name, ICurve curve, ILifetime<PrimitiveMesh> mesh, BoundingBox bounds)
    {
        this.Entity = entity;
        this.Name = name;
        this.Curve = curve;
        this.Mesh = mesh;
        this.Bounds = bounds;
        this.Instances = new List<TrackInstance>();
        this.IsDirty = false;
    }

    public Entity Entity { get; }
    public string Name { get; }
    public ICurve Curve { get; }
    public ILifetime<PrimitiveMesh> Mesh { get; }
    public BoundingBox Bounds { get; }

    public List<TrackInstance> Instances { get; }

    public bool IsDirty { get; set; }
}
