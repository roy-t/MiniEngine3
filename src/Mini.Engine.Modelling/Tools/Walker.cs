using System.Numerics;
using Mini.Engine.Graphics;

namespace Mini.Engine.Modelling.Tools;
public static class Walker
{
    public static Transform[] Walk(Path3D layout, float stepSize)
    {
        var length = layout.IsClosed ? layout.Length : layout.Length - 1;

        var totalLength = 0.0f;
        for (var i = 0; i < length; i++)
        {
            totalLength += Vector3.Distance(layout[i], layout[i + 1]);
        }

        var transforms = new Transform[(int)(totalLength / stepSize)];
        var counter = 0;
        var accumulator = 0.0f;
        for (var i = 0; i < length; i++)
        {
            var start = layout[i];
            var end = layout[i + 1];
            var normal = layout.GetForward(i);

            accumulator += Vector3.Distance(start, end);

            while (accumulator > stepSize && counter < transforms.Length)
            {
                accumulator -= stepSize;

                start += normal * stepSize;
                transforms[counter++] = new Transform(start, Quaternion.Identity, Vector3.Zero, 1.0f).FaceTarget(start + normal);
            }
        }

        return transforms;
    }
}
