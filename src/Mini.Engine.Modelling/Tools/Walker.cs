using System.Diagnostics;
using System.Numerics;
using Mini.Engine.Graphics;

namespace Mini.Engine.Modelling.Tools;
public static class Walker
{
    /// <summary>
    /// Returns evenly spaced out transforms on the given path, respecting the minStepSize and making sure that
    /// the first and last item are at minStepSize*0.5f away from their ends so multiple path can be combined without visible distortion
    /// </summary>
    public static Transform[] WalkSpacedOut(Path3D layout, float minStepSize, Vector3 up)
    {
        Debug.Assert(minStepSize > 0.0f);

        var totalLength = 0.0f;
        for (var i = 0; i < layout.Steps; i++)
        {
            totalLength += Vector3.Distance(layout[i], layout[i + 1]);
        }

        Debug.Assert(totalLength > minStepSize);

        // Make sure the piece fits perfectly to another piece that uses the same stepsize
        totalLength -= minStepSize;
        var startOffset = minStepSize * 0.5f;

        var transforms = new Transform[(int)(totalLength / minStepSize) + 1];
                
        // Even out the spacing between all items so that we get rid of the remainder
        var remainder = totalLength % minStepSize;
        var stepSize = minStepSize + (remainder / (transforms.Length - 1));

        for (var i = 0; i < transforms.Length; i++)
        {
            var position = layout.GetPositionAfterDistance((i * stepSize) + startOffset);
            var normal = layout.GetForwardAfterDistance((i * stepSize) + startOffset);

            transforms[i] = new Transform(position, Quaternion.Identity, Vector3.Zero, 1.0f).FaceTargetConstrained(position + normal, up);
        }

        return transforms;
    }
}
