using System.Numerics;
using Mini.Engine.Graphics;

namespace Mini.Engine.Modelling.Tools;
public static class Walker
{
    public static Transform[] Walk(Path3D layout, float stepSize, Vector3 up)
    {
        var totalLength = 0.0f;
        for (var i = 0; i < layout.Steps; i++)
        {
            totalLength += Vector3.Distance(layout[i], layout[i + 1]);
        }

        var transforms = new Transform[(int)(totalLength / stepSize)];

        for (var i = 0; i < transforms.Length; i++)
        {
            var position = layout.GetPositionAfterDistance(i * stepSize);
            var normal = layout.GetForwardAfterDistance(i * stepSize);

            transforms[i] = new Transform(position, Quaternion.Identity, Vector3.Zero, 1.0f).FaceTargetConstrained(position + normal, up);
        }

        return transforms;
    }
}
