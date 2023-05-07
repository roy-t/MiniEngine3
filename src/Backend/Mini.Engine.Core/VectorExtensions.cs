using System.Numerics;

namespace Mini.Engine.Core;
public static class VectorExtensions
{
    public static Vector3 ToVector3(this Vector2 vector, float z = 0.0f)
    {
        return new Vector3(vector, z);
    }
}
