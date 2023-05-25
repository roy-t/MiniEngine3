using System.Numerics;

namespace Mini.Engine.Modelling.Curves;
public interface ICurve
{
    public Vector2 GetPosition(float u, float amplitude);

    public float ComputeLength(float amplitude);
}
