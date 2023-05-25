using System.Diagnostics;
using System.Numerics;
using Mini.Engine.Graphics;
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
        Debug.Assert(minStepSize > 0.0f);
        
        var curveLength = curve.ComputeLength();        

        Debug.Assert(curveLength > minStepSize);

        var totalLength = curveLength - minStepSize;
        var startOffset = minStepSize * 0.5f;

        var transforms = new Transform[(int)(totalLength / minStepSize) + 1];
        
        //// Even out the spacing between all items so that we get rid of the remainder
        var remainder = totalLength % minStepSize;
        var stepSize = minStepSize + (remainder / (transforms.Length - 1));

        var scale = 1.0f / curveLength;

        for (var i = 0; i < transforms.Length; i++)
        {     
            var p = curve.GetPosition3D(((i * stepSize) + startOffset) * scale);            
            var n = curve.GetNormal3D(((i * stepSize) + startOffset) * scale);

            transforms[i] = new Transform(p, Quaternion.Identity, Vector3.Zero, 1.0f).FaceTargetConstrained(p + n, up);
        }

        return transforms;
    }
}
