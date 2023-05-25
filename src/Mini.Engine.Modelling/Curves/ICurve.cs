using System.Numerics;

namespace Mini.Engine.Modelling.Curves;
public interface ICurve
{
    public Vector2 GetPosition(float u);
    public Vector2 GetNormal(float u);

    public float ComputeLength();
}
