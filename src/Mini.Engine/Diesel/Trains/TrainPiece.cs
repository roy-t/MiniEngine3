using LibGame.Physics;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.Graphics.Primitives;
using Vortice.Mathematics;

namespace Mini.Engine.Diesel.Trains;
public sealed class TrainPiece
{
    public TrainPiece(ILifetime<PrimitiveMesh> mesh, BoundingBox bounds, Transform offset)
    {
        this.Mesh = mesh;
        this.Bounds = bounds;
        this.Offset = offset;
    }

    public ILifetime<PrimitiveMesh> Mesh { get; }
    public BoundingBox Bounds { get; }
    public Transform Offset { get; }
}
