using System.Numerics;
using Xunit;

namespace Mini.Engine.Tests;
public static class FloatAssert
{
    public static void AlmostEqual(Vector3 expected, Vector3 actual, float tolerance = 0.001f)
    {
        var dX = Math.Abs(expected.X - actual.X);
        var dY = Math.Abs(expected.Y - actual.Y);
        var dZ = Math.Abs(expected.Z - actual.Z);

        if (dX > tolerance || dY > tolerance || dZ > tolerance)
        {
            Assert.Equal(expected, actual);
        }
    }
}
