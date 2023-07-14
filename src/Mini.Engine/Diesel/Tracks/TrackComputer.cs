using System.Numerics;
using LibGame.Mathematics;
using LibGame.Physics;
using Mini.Engine.Configuration;
using Mini.Engine.Modelling.Curves;
using Vortice.Mathematics;

namespace Mini.Engine.Diesel.Tracks;

public readonly record struct TrackCurveInstanceId(int TrackId, int CurveId);
public readonly record struct Connection(int FromCurveId, int UFrom, int ToCurveId, int UTo);
public readonly record struct Orientation(Vector3 Offset, float Yaw);

public readonly record struct CurvePlacement(int Id, Transform Transform, Vector3 StartPosition, Vector3 StartForward, Vector3 EndPosition, Vector3 EndForward);

[Service]
public sealed class TrackComputer
{
    private const float MAX_CONNECT_DISTANCE = 0.1f;

    private readonly CurveManager Curves;

    private readonly Dictionary<int, CurvePlacement> Placements;
    private readonly Dictionary<int, Connection> OutgoingConnections;
    private readonly Dictionary<int, Connection> IncomingConnections;

    private int lastCurveId;

    public TrackComputer(CurveManager curves)
    {
        this.Curves = curves;

        this.Placements = new Dictionary<int, CurvePlacement>();
        this.OutgoingConnections = new Dictionary<int, Connection>();
        this.IncomingConnections = new Dictionary<int, Connection>();

        this.lastCurveId = 0;
    }

    public CurvePlacement PlaceCurve(Vector3 position, Vector3 forward, ICurve curve)
    {
        var yaw = Radians.YawFromVector(forward);
        var nextYaw = Radians.YawFromVector(curve.GetForward(0.0f));
        var difference = Radians.DistanceRadians(nextYaw, yaw);

        var rotation = Quaternion.CreateFromYawPitchRoll(difference, 0.0f, 0.0f);
        var origin = curve.GetPosition(0.0f);
        var transform = new Transform(position, rotation, origin, 1.0f);

        var endPosition = position + Vector3.Transform(curve.GetPosition(1.0f) - curve.GetPosition(0.0f), transform.GetRotation());
        var endForward = Vector3.Normalize(Vector3.Transform(curve.GetForward(1.0f), transform.GetRotation()));


        var placement = new CurvePlacement(++this.lastCurveId, transform, position, forward, endPosition, endForward);
        this.Placements.Add(placement.Id, placement);

        //this.ConnectCurve(in placement);

        return placement;
    }


    //private void ConnectCurve(in CurvePlacement placement)
    //{
    //    foreach (var curve in this.Placements.Values)
    //    {
    //        var distance = Vector3.DistanceSquared(placement.StartForward, curve.StartForward);

    //        var start = curve.StartForward;
    //        var end = curve.EndForward;


    //        if (curve.StartForward)
    //    }
    //}

    public TrackCurveInstanceId AddInstance(in Orientation orientation, ICurve curve)
    {
        throw new NotImplementedException();
    }

    public void GetConnections(TrackCurveInstanceId id, IList<Connection> output)
    {
        throw new NotImplementedException();
    }

    public Orientation GetOrientation(TrackCurveInstanceId id, float u)
    {
        throw new NotImplementedException();
    }

    public bool PickTrack(Ray ray, out TrackCurveInstanceId id)
    {
        throw new NotImplementedException();
    }
}
