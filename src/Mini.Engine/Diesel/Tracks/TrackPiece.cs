using System.Numerics;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.ECS;
using Mini.Engine.Graphics.Primitives;
using Mini.Engine.Modelling.Curves;
using Vortice.Mathematics;

namespace Mini.Engine.Diesel.Tracks;
public sealed class TrackPiece
{
    public TrackPiece(Entity entity, string name, ICurve curve, ILifetime<PrimitiveMesh> mesh, BoundingBox bounds)
    {
        this.Entity = entity;
        this.Name = name;
        this.Curve = curve;
        this.Mesh = mesh;
        this.Bounds = bounds;
        this.Instances = new List<Matrix4x4>();
        this.IsDirty = false;
    }

    public Entity Entity { get; }
    public string Name { get; }
    public ICurve Curve { get; }
    public ILifetime<PrimitiveMesh> Mesh { get; }
    public BoundingBox Bounds { get; }

    public List<Matrix4x4> Instances { get; }

    public bool IsDirty { get; set; }
}
