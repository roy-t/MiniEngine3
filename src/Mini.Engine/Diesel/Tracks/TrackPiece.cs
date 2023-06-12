using Mini.Engine.Core.Lifetime;
using Mini.Engine.Graphics.Primitives;
using Mini.Engine.Modelling.Curves;
using Vortice.Mathematics;

namespace Mini.Engine.Diesel.Tracks;
public sealed class TrackPiece
{    
    public TrackPiece(ICurve curve, ILifetime<PrimitiveMesh> mesh, BoundingBox bounds)
    {
        this.Curve = curve;
        this.Mesh = mesh;
        this.Bounds = bounds;
    }

    public ICurve Curve { get; }
    public ILifetime<PrimitiveMesh> Mesh { get; }
    public BoundingBox Bounds { get; }
}
