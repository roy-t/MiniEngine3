using System.Numerics;
using LibGame.Mathematics;
using LibGame.Physics;
using Mini.Engine.Configuration;
using Mini.Engine.Modelling.Curves;
using Vortice.Mathematics;

namespace Mini.Engine.Diesel.Tracks;

public readonly record struct TrackCurveInstanceId(int TrackId, int CurveId);
public readonly record struct Connection(int TrackIdFrom, int CurveIdFrom, int UFrom, int TrackIdTo, int CurveIdTo, int UTo);
public readonly record struct Orientation(Vector3 Offset, float Yaw);

public readonly record struct CurvePlacement(int Id, Transform Transform, Vector3 StartPosition, Vector3 StartForward, Vector3 EndPosition, Vector3 EndForward);

[Service]
public sealed class TrackComputer
{
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

        return new CurvePlacement(0, transform, position, forward, endPosition, endForward);
    }

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

    public bool PickTack(Ray ray, out TrackCurveInstanceId id)
    {
        throw new NotImplementedException();
    }
}
