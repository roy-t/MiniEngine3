using System.Diagnostics;
using System.Numerics;

namespace Mini.Engine.Modelling.Curves;
public sealed class SegmentedCurve : ICurve
{
    private readonly IReadOnlyList<ICurve> Curves;
    private readonly IReadOnlyList<Matrix4x4> Transforms;
    private readonly IReadOnlyList<float> EndUs;

    public SegmentedCurve(IReadOnlyList<ICurve> curves, IReadOnlyList<Matrix4x4> transforms)
    {
        this.Curves = curves;

        for (var i = 0; i < curves.Count; i++)
        {
            this.Length += curves[i].Length;
        }

        var endUs = new float[curves.Count];
        var accumulated = 0.0f;
        for (var i = 0; i < curves.Count; i++)
        {
            accumulated += curves[i].Length;
            endUs[i] = accumulated / this.Length;
        }

        this.EndUs = endUs;
        this.Transforms = transforms;
    }

    public Vector3 GetPosition(float u)
    {
        var (index, su) = this.GetSegment(u);
        var transform = this.Transforms[index];
        return Vector3.Transform(this.Curves[index].GetPosition(su), transform);
    }

    public Vector3 GetForward(float u)
    {
        var (index, su) = this.GetSegment(u);
        var transform = this.Transforms[index];
        return Vector3.TransformNormal(this.Curves[index].GetForward(su), transform);
    }

    public float Length { get; }
    public int Count => this.Curves.Count;
    public ICurve this[int index] => this.Curves[index];


    private (int Index, float U) GetSegment(float u)
    {
        Debug.Assert(u >= 0 && u <= 1.0f);
        
        var index = 0;
        while (this.EndUs[index] < u)
        {
            index++;
        }

        // We know u is somewhere on this segment
        
        var startU = index > 0 ? this.EndUs[index - 1] : 0.0f;
        var startLength = this.Length * startU;
        var endU = index < this.Count ? this.EndUs[index] : 1.0f;
        var endLength = this.Length * endU;
        var expected = (this.Length * u) - startLength;
        var segmentLength = endLength - startLength;

        var finalU = expected / segmentLength;
        

        return (index, finalU);
    }
}
