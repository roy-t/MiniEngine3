using System.Numerics;
using Mini.Engine.Modelling.Curves;
using Xunit;
using static Mini.Engine.Tests.FloatAssert;
using static Xunit.Assert;

namespace Mini.Engine.Tests;
public static class CurveTests
{

    [Fact(DisplayName = "`Segmented` curve should proportionally index the underlying curves")]
    public static void SegmentedCurve()
    {
        var a = new StraightCurve(new Vector3(0, 0, 0), -Vector3.UnitZ, 1.0f);

        var ta = Matrix4x4.Identity;
        var tb = Matrix4x4.CreateTranslation(new Vector3(0, 0, -1));
        var tc = Matrix4x4.CreateTranslation(new Vector3(0, 0, -2));

        var curves = new[] { a, a, a };
        var transforms = new[] { ta, tb, tc };
        
        var segmented = new SegmentedCurve(curves, transforms);

        AlmostEqual(new Vector3(0, 0, -0.00f), segmented.GetPosition(0.0f));
        AlmostEqual(new Vector3(0, 0, -1.00f), segmented.GetPosition(0.33333f));
        AlmostEqual(new Vector3(0, 0, -2.00f), segmented.GetPosition(0.66666f));
        AlmostEqual(new Vector3(0, 0, -2.25f), segmented.GetPosition(0.75f));
        AlmostEqual(new Vector3(0, 0, -3.00f), segmented.GetPosition(1.0f));
    }

    [Fact(DisplayName = "`TravelAlongCurve` should return a position on the curve that is the given distance along the curve away from the starting position")]
    public static void TravelAlongCurve()
    {
        var straight = new StraightCurve(Vector3.Zero, -Vector3.UnitZ, 10.0f);

        True(straight.TravelAlongCurve(0, 5.0f, out var uNext));
        Equal(0.5f, uNext, 0.01f);

        True(straight.TravelAlongCurve(0.5f, 2.5f, out uNext));
        Equal(0.75f, uNext, 0.01f);

        False(straight.TravelAlongCurve(0.0f, 11.0f, out uNext));

        var arc = new CircularArcCurve(0.0f, MathF.PI, 1.0f);
        True(arc.TravelAlongCurve(0.0f, MathF.PI * 0.5f, out uNext));
        Equal(0.5f, uNext, 0.01f);
    }

    [Fact(DisplayName ="`TravelEucledianDistance` should return a position on the curven that is the given Eucledian distance away from the starting position")]
    public static void TravelEucledianDistance()
    {
        var arc = new CircularArcCurve(0.0f, MathF.PI, 1.0f);

        var requiredDistance = 2.0f;
        var margin = 0.01f;        
        True(arc.TravelEucledianDistance(0.0f, requiredDistance, margin, out var uEnd));
        var distance = Vector3.Distance(arc.GetPosition(0), arc.GetPosition(uEnd));

        InRange(distance, requiredDistance - margin, requiredDistance + margin);        
    }
}
