using System.Diagnostics;
using System.Numerics;

namespace Mini.Engine.Modelling.Curves;
public sealed class SegmentedCurve : ICurve
{
    private readonly IReadOnlyList<Matrix4x4> Transforms;
    private readonly IReadOnlyList<float> EndUs;

    public SegmentedCurve(IReadOnlyList<ICurve> curves, IReadOnlyList<Matrix4x4> transforms)
    {
        this.Segments = curves;

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

    public IReadOnlyList<ICurve> Segments { get; }

    public Vector3 GetPosition(float u)
    {
        var (index, su) = this.GetSegment(u);
        var transform = this.Transforms[index];
        return Vector3.Transform(this.Segments[index].GetPosition(su), transform);
    }

    public Vector3 GetForward(float u)
    {
        var (index, su) = this.GetSegment(u);
        var transform = this.Transforms[index];
        return Vector3.TransformNormal(this.Segments[index].GetForward(su), transform);
    }

    public float GetEndOfSegment(int segment)
    {
        if (segment < this.Segments.Count)
        {
            return this.EndUs[segment];
        }

        return 1.0f;
    }

    public float GetStartOfSegment(int segment)
    {
        if (segment == 0)
        {
            return 0.0f;
        }

        return this.EndUs[segment - 1];
    }

    public float Length { get; }
    public int Count => this.Segments.Count;
    public ICurve this[int index] => this.Segments[index];


    private (int Index, float U) GetSegment(float u)
    {
        Debug.Assert(u >= 0 && u <= 1.0f);

        var index = 0;
        while (this.EndUs[index] < u)
        {
            index++;
        }

        // We know u is somewhere on this segment

        var startU = this.GetStartOfSegment(index);
        var startLength = this.Length * startU;

        var endU = this.GetEndOfSegment(index);        
        var endLength = this.Length * endU;

        var segmentLength = endLength - startLength;

        var expected = (this.Length * u) - startLength;
        
        var finalU = expected / segmentLength;

        return (index, finalU);
    }
}
