using System.Numerics;
using LibGame.Physics;
using Mini.Engine.Configuration;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;
using Mini.Engine.Graphics.Lighting.ShadowingLights;
using Mini.Engine.Graphics.Primitives;
using Mini.Engine.Graphics.Transforms;
using Mini.Engine.Modelling.Curves;
using static Mini.Engine.Diesel.Tracks.TrackParameters;

namespace Mini.Engine.Diesel.Tracks;

// TODO: maybe we need to move making all the track pieces and components out of this class?

[Service]
public sealed class TrackManager
{
    private const int BufferCapacity = 100;

    private readonly Device Device;
    private readonly ECSAdministrator Administrator;
    private readonly TrackGrid Grid;
    private readonly IComponentContainer<InstancesComponent> Instances;

    private readonly List<TrackPiece> Pieces;

    public readonly TrackPiece Straight;
    public readonly TrackPiece LeftTurn;
    public readonly TrackPiece RightTurn;

    public TrackManager(Device device, ECSAdministrator administrator, CurveManager curves, IComponentContainer<InstancesComponent> instances)
    {
        this.Device = device;
        this.Administrator = administrator;
        this.Grid = new TrackGrid(100, 100, STRAIGHT_LENGTH, STRAIGHT_LENGTH);
        this.Instances = instances;

        this.Straight = this.CreateTrackPieceAndComponents(device, curves.Straight, STRAIGHT_VERTICES, nameof(this.Straight));
        this.LeftTurn = this.CreateTrackPieceAndComponents(device, curves.LeftTurn, TURN_VERTICES, nameof(this.LeftTurn));
        this.RightTurn = this.CreateTrackPieceAndComponents(device, curves.RightTurn, TURN_VERTICES, nameof(this.RightTurn));

        this.Pieces = new List<TrackPiece>()
        {
            this.Straight,
            this.LeftTurn,
            this.RightTurn,
        };
    }    

    public void Clear()
    {
        foreach (var trackPiece in this.Pieces)
        {
            ref var component = ref this.Instances[trackPiece.Entity];
            component.Value.InstanceList.Clear();
            component.LifeCycle = component.LifeCycle.ToChanged();
        }
    }

    public (Matrix4x4, ICurve) AddStraight(Vector3 approximatePosition, Vector3 forward)
    {
        var offset = this.Place(this.Straight.Curve, approximatePosition, forward);
        this.AddInstance(this.Straight, offset);

        return (offset, this.Straight.Curve);
    }

    public (Matrix4x4, ICurve) AddLeftTurn(Vector3 approximatePosition, Vector3 forward)
    {
        var offset = this.Place(this.LeftTurn.Curve, approximatePosition, forward);
        this.AddInstance(this.LeftTurn, offset);

        return (offset, this.LeftTurn.Curve);
    }

    public (Matrix4x4, ICurve) AddRightTurn(Vector3 approximatePosition, Vector3 forward)
    {
        var offset = this.Place(this.RightTurn.Curve, approximatePosition, forward);
        this.AddInstance(this.RightTurn, offset);

        return (offset, this.RightTurn.Curve);
    }

    private Matrix4x4 Place(ICurve curve, Vector3 approximatePosition, Vector3 forward)
    {
        // Find the cell the curve needs to be placed in
        var (x, y) = this.Grid.PickCell(approximatePosition);

        // Find a position on the border of the cell, backwards from the picked position
        var (cellMin, cellMax) = this.Grid.GetCellBounds(x, y);
        var midX = (cellMax.X + cellMin.X) / 2.0f;
        var midY = (cellMax.Y + cellMin.Y) / 2.0f;
        var midZ = (cellMax.Z + cellMin.Z) / 2.0f;

        Vector3 position;

        // Forward is pointing forward, start at the center of the 'backward' edge
        if (Vector3.Dot(forward, new Vector3(0, 0, -1)) > 0.95f)
        {
            position = new Vector3(midX, midY, cellMax.Z);
        }
        // Forward is pointing back, start at the center of the 'forward' edge
        else if (Vector3.Dot(forward, new Vector3(0, 0, 1)) > 0.95f)
        {
            position = new Vector3(midX, midY, cellMin.Z);
        }
        // Forward is pointing right, start at the center of 'left' edge
        else if (Vector3.Dot(forward, new Vector3(1, 0, 0)) > 0.95f)
        {
            position = new Vector3(cellMin.X, midY, midZ);
        }
        // Forward is pointing left, start at the center of 'right' edge
        else if (Vector3.Dot(forward, new Vector3(-1, 0, 0)) > 0.95f)
        {
            position = new Vector3(cellMax.X, midY, midZ);
        }
        else
        {
            throw new NotImplementedException("Unexpected direction");
        }

        var transform = curve.PlaceInXZPlane(0.0f, position, forward);

        this.Grid.Add(x, y, curve, transform);

        return transform.GetMatrix();
    }

    private void AddInstance(TrackPiece trackPiece, Matrix4x4 offset)
    {
        ref var component = ref this.Instances[trackPiece.Entity];
        component.Value.InstanceList.Add(offset);
        component.LifeCycle = component.LifeCycle.ToChanged();
    }

    private TrackPiece CreateTrackPieceAndComponents(Device device, ICurve curve, int points, string name)
    {
        var entity = this.Administrator.Entities.Create();
        var trackPiece = new TrackPiece(entity, name, curve);
        var primitive = TrackPieces.FromCurve(device, curve, points, name);

        this.CreateComponents(entity, primitive);

        return trackPiece;
    }

    private void CreateComponents(Entity entity, ILifetime<PrimitiveMesh> mesh)
    {
        var components = this.Administrator.Components;

        ref var transform = ref components.Create<TransformComponent>(entity);
        transform.Current = Transform.Identity;
        transform.Previous = transform.Current;

        ref var primitive = ref components.Create<PrimitiveComponent>(entity);
        primitive.Mesh = mesh;

        ref var shadows = ref components.Create<ShadowCasterComponent>(entity);
        shadows.Importance = 1.0f; // TODO: figure out which parts do and do not need a shadow

        ref var instances = ref components.Create<InstancesComponent>(entity);
        instances.Init(this.Device, $"Instances{entity}", BufferCapacity);
    }
}
