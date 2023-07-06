using System.Numerics;

namespace Mini.Engine.Modelling.Curves;
public interface ICurve
{
    public Vector3 GetPosition(float u);
    public Vector3 GetForward(float u);

    public float ComputeLength();
}
