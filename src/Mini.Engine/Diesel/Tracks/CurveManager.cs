using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Modelling.Curves;

using static Mini.Engine.Diesel.Tracks.TrackParameters;

namespace Mini.Engine.Diesel.Tracks;

[Service]
public sealed class CurveManager
{
    private readonly ICurve[] Curves;

    public CurveManager()
    {
        this.LeftTurn = new CircularArcCurve(0.0f, MathF.PI / 2.0f, TURN_RADIUS).Reverse();
        this.RightTurn = new CircularArcCurve(0.0f, MathF.PI / 2.0f, TURN_RADIUS);
        this.Straight = new StraightCurve(new Vector3(0.0f, 0.0f, STRAIGHT_LENGTH * 0.5f), new Vector3(0.0f, 0.0f, -1.0f), STRAIGHT_LENGTH);

        this.Curves = new ICurve[]
        {
            this.LeftTurn,
            this.RightTurn,
            this.Straight
        };
    }

    public ICurve LeftTurn { get; }
    public ICurve RightTurn { get; }
    public ICurve Straight { get; }

    public ICurve Get(int curveId)
    {
        return this.Curves[curveId];
    }

    public int GetId(ICurve curve)
    {
        for (var i = 0; i < this.Curves.Length; i++)
        {
            if (this.Curves[i] == curve)
            {
                return i;
            }
        }

        throw new Exception("Unknown curve");
    }
}
