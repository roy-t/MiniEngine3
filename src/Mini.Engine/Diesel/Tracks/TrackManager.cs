﻿using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;
using Mini.Engine.Graphics.Primitives;
using Mini.Engine.Modelling.Curves;
using static Mini.Engine.Diesel.Tracks.TrackParameters;

namespace Mini.Engine.Diesel.Tracks;

[Service]
public sealed class TrackManager
{
    private const int BufferCapacity = 100;

    private readonly ECSAdministrator Administrator;
    private readonly InstancesSystem InstancesSystem;
    private readonly TrackGrid Grid;

    private readonly List<TrackPiece> Pieces;

    public readonly TrackPiece Straight;
    public readonly TrackPiece LeftTurn;
    public readonly TrackPiece RightTurn;

    public TrackManager(Device device, ECSAdministrator administrator, CurveManager curves, ScenarioManager scenarioManager, InstancesSystem instancesSystem, IComponentContainer<InstancesComponent> instances)
    {
        this.Administrator = administrator;
        this.InstancesSystem = instancesSystem;
        this.Grid = scenarioManager.Grid;

        // TODO: because we reference an entity here that gets deleted when we changes scenes, this breaks!
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
            trackPiece.Instances.Clear();
            this.InstancesSystem.QueueUpdate(trackPiece.Entity, trackPiece.Instances);
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
        trackPiece.Instances.Add(offset);
        this.InstancesSystem.QueueUpdate(trackPiece.Entity, trackPiece.Instances);
    }

    private TrackPiece CreateTrackPieceAndComponents(Device device, ICurve curve, int points, string name)
    {
        var primitive = TrackPieces.FromCurve(device, curve, points, name);
        var entity = PrimitiveUtilities.CreateComponents(device, this.Administrator, primitive, BufferCapacity, 1.0f);
        return new TrackPiece(entity, name, curve);
    }
}
