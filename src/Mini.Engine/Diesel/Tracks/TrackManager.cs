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

    private readonly ECSAdministrator Administrator;
    private readonly TrackGrid Grid;
    private readonly IComponentContainer<InstancesComponent> Instances;

    private readonly List<TrackPiece> Pieces;

    public readonly TrackPiece Straight;
    public readonly TrackPiece LeftTurn;
    public readonly TrackPiece RightTurn;

    public TrackManager(Device device, ECSAdministrator administrator, CurveManager curves, ScenarioManager scenarioManager, IComponentContainer<InstancesComponent> instances)
    {
        this.Administrator = administrator;
        this.Grid = scenarioManager.Grid;
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
        var offset = this.Grid.Add(this.Straight.Curve, approximatePosition, forward).GetMatrix();
        this.AddInstance(this.Straight, offset);

        return (offset, this.Straight.Curve);
    }

    public (Matrix4x4, ICurve) AddLeftTurn(Vector3 approximatePosition, Vector3 forward)
    {
        var offset = this.Grid.Add(this.LeftTurn.Curve, approximatePosition, forward).GetMatrix();
        this.AddInstance(this.LeftTurn, offset);

        return (offset, this.LeftTurn.Curve);
    }

    public (Matrix4x4, ICurve) AddRightTurn(Vector3 approximatePosition, Vector3 forward)
    {
        var offset = this.Grid.Add(this.RightTurn.Curve, approximatePosition, forward).GetMatrix();
        this.AddInstance(this.RightTurn, offset);

        return (offset, this.RightTurn.Curve);
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

        this.CreateComponents(device, entity, primitive);

        return trackPiece;
    }

    private void CreateComponents(Device device, Entity entity, ILifetime<PrimitiveMesh> mesh)
    {
        var components = this.Administrator.Components;

        ref var transform = ref components.Create<TransformComponent>(entity);
        transform.Current = Transform.Identity;
        transform.Previous = transform.Current;

        ref var primitive = ref components.Create<PrimitiveComponent>(entity);
        primitive.Mesh = mesh;

        ref var shadows = ref components.Create<ShadowCasterComponent>(entity);
        shadows.Importance = 1.0f;

        ref var instances = ref components.Create<InstancesComponent>(entity);
        instances.Init(device, $"Instances{entity}", BufferCapacity);
    }
}
