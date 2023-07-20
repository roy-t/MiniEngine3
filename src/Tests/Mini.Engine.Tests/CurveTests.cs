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

        // TODO: should we make a transformed curve instead?
        var segmented = new SegmentedCurve(curves, transforms);

        AlmostEqual(new Vector3(0, 0, -0.00f), segmented.GetPosition(0.0f));
        AlmostEqual(new Vector3(0, 0, -1.00f), segmented.GetPosition(0.33333f));
        AlmostEqual(new Vector3(0, 0, -2.00f), segmented.GetPosition(0.66666f));
        AlmostEqual(new Vector3(0, 0, -2.25f), segmented.GetPosition(0.75f));
        AlmostEqual(new Vector3(0, 0, -3.00f), segmented.GetPosition(1.0f));
    }
}
