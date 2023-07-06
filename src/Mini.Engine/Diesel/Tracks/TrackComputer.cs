using System.Numerics;
using LibGame.Mathematics;
using Mini.Engine.Modelling.Curves;
using Vortice.Mathematics;

namespace Mini.Engine.Diesel.Tracks;

public readonly record struct TrackCurveInstanceId(int TrackId, int CurveId);
public readonly record struct Connection(int TrackIdFrom, int CurveIdFrom, int UFrom, int TrackIdTo, int CurveIdTo, int UTo);
public readonly record struct Orientation(Vector3 Offset, float Yaw);

public sealed class TrackComputer
{
    private const float SNAP_DISTANCE = 0.05f;

    public static Orientation PlaceCurve(Vector3 position, Vector3 forward, ICurve curve, float u)
    {
        var nextPosition = curve.GetPosition(u);
        var nextForward = curve.GetForward(u);

        var nextYaw = Radians.YawFromVector(nextForward);
        var yaw = Radians.YawFromVector(forward);

        var distance = Radians.DistanceRadians(yaw, nextYaw);

        var offset = new Vector3(position.X - nextPosition.X, 0.0f, position.Y - nextPosition.Y);
        return new Orientation(offset, distance);
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
