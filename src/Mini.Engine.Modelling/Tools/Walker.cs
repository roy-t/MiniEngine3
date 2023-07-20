using System.Numerics;
using LibGame.Physics;
using Mini.Engine.Modelling.Curves;

namespace Mini.Engine.Modelling.Tools;
public static class Walker
{
    /// <summary>
    /// Returns evenly spaced out transforms on the given path, respecting the minStepSize and making sure that
    /// the first and last item are at minStepSize*0.5f away from their ends so multiple path can be combined without visible distortion
    /// </summary>
    public static Transform[] WalkSpacedOut(ICurve curve, float minStepSize, Vector3 up)
    {
        var length = curve.Length;
        var shorten = minStepSize / length;
        curve = curve.Range(shorten * 0.5f, 1.0f - shorten);

        length = curve.Length;
        var intervals = (int)(length / minStepSize);        
        var remainder = length - (minStepSize * intervals);
        var stepSize = minStepSize + (remainder / intervals);
        var u = stepSize / length;

        var transforms = new Transform[intervals + 1];
        for (var i = 0; i < transforms.Length; i++)
        {
            var p = curve.GetPosition(i * u);
            var n = curve.GetForward(i * u);

            transforms[i] = new Transform(p, Quaternion.Identity, Vector3.Zero, 1.0f).FaceTargetConstrained(p + n, up);
        }

        return transforms;
    }
}
